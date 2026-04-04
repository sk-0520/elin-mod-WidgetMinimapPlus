using Elin.Plugin.Generated;
using Elin.Plugin.Main.Models.Settings;
using Elin.Plugin.Main.PluginHelpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

namespace Elin.Plugin.Main.Models.Impl
{
    public static class WidgetMinimapImpl
    {
        #region define

        private const float BillboardSizeScale = 3.4f;

        #endregion

        #region variable

        private static int emitSequence = 0;

        #endregion

        #region property

        private static Marker StairUpMarker { get; } = new Marker()
        {
            MarkerShape = MarkerShape.TriangleUp,
        };

        private static Marker StairDownMarker { get; } = new Marker()
        {
            MarkerShape = MarkerShape.TriangleDown,
        };

        private static Marker MinionMarker { get; } = new Marker()
        {
            MarkerShape = MarkerShape.Diamond
        };

        private static Marker PetMarker { get; } = new Marker()
        {
            MarkerShape = MarkerShape.Diamond
        };

        private static Marker SpecialCharacterMarker { get; } = new Marker()
        {
            MarkerShape = MarkerShape.Pentagon,
        };

        private static Marker SpecialThingMarker { get; } = new Marker()
        {
            MarkerShape = MarkerShape.Star,
        };

        private static Marker CustomMarker { get; } = new Marker()
        {
            MarkerShape = MarkerShape.Cross,
        };

        private static readonly IReadOnlyCollection<Marker> Markers = [
            StairUpMarker, StairDownMarker,
            MinionMarker,
            PetMarker,
            SpecialCharacterMarker,
            SpecialThingMarker,
            CustomMarker,
        ];

        private static TooltipData TooltipData { get; set; } = new TooltipData()
        {
            id = "note",
            enable = false,
        };

        private static CustomHitCache CustomHitCache { get; } = new CustomHitCache();

        #endregion

        #region function

        private static void Reset(WidgetMinimap instance)
        {
            ModHelper.LogDebug("Resetting minimap markers");

            emitSequence = 0; // これは意味ないけど一応ね

            foreach (var marker in Markers)
            {
                var ps = marker.ParticleSystem;
                if (ps == null)
                {
                    continue;
                }

                try
                {
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        var matInstance = renderer.material;
                        if (matInstance != null)
                        {
                            Object.Destroy(matInstance);
                        }
                    }

                    Object.Destroy(ps.gameObject);
                }
                catch (System.Exception ex)
                {
                    ModHelper.LogNotExpected(ex);
                }
                finally
                {
                    marker.ParticleSystem = null;
                }
            }
        }

        //Input.mousePosition, ELayer.ui.canvas.worldCamera
        private static bool TryGetMinimapPointToCellPosition(WidgetMinimap instance, Vector3 minimapPoint, Camera camera, out Point2D result)
        {
            // [ELIN:WidgetMinimap.OnPointerDown]
            // -> Vector2 size = rectMap.rect.size
            Vector2 size = instance.rectMap.rect.size;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(instance.rectMap, minimapPoint, camera, out var localPoint))
            {
                result = default;
                return false;
            }
            var ignore = localPoint.x < 0f || localPoint.x >= size.x || localPoint.y < 0f || localPoint.y >= size.y;
            if (ignore)
            {
                result = default;
                return false;
            }

            var pos = new Point();
            pos.Set((int)((float)instance.Size * localPoint.x / size.x), (int)((float)instance.Size * localPoint.y / size.y));
            pos.Clamp();
            if (instance.preview.limitBounds)
            {
                pos.x += EMono._map.bounds.x - instance.preview.offsetX;
                pos.z += EMono._map.bounds.z - instance.preview.offsetZ;
            }

            // [ELIN:WidgetMinimap.OnPointerDown]
            // -> pos.cell.outOfBounds || !pos.cell.isSeen
            if (pos.cell.outOfBounds)
            {
                result = default;
                return false;
            }

            result = new Point2D(pos.x, pos.z);
            return true;
        }

        /// <summary>
        /// 検索対象のキャラクター一覧を取得。
        /// </summary>
        /// <param name="characters"></param>
        /// <param name="judgement"></param>
        /// <returns></returns>
        private static IEnumerable<Chara> FilterSearchableCharacters(IEnumerable<Chara> characters, Judgement judgement)
        {
            return characters.Where(c => judgement.IsSearchableCharacter(c));
        }

        /// <summary>
        /// 検索対象のモノ一覧を取得。
        /// </summary>
        /// <param name="things"></param>
        /// <param name="judgement"></param>
        /// <returns></returns>
        private static IEnumerable<Thing> FilterSearchableThings(IEnumerable<Thing> things, Judgement judgement)
        {
            return things.Where(t => judgement.IsSearchableThing(t));
        }

        private static Material CreateMaterial(WidgetMinimap instance, Scene scene, Zone zone, Material sourceMaterial, MarkerShape markerShape)
        {
            var modMaterial = new Material(sourceMaterial);

            if (modMaterial.HasProperty("_MainTex"))
            {
                var shapeTexture = TextureUtility.CreateShapeTexture(markerShape, 128);

                // [ELIN:WidgetMinimap.RefreshStyle]
                // -> rectAll.localEulerAngles = ((extra.rotate && !EMono._zone.IsRegion) ? new Vector3(60f, 0f, -45f) : Vector3.zero);
                if (instance.extra.rotate && !ModHelper.Elin.IsGlobalMap(scene, zone))
                {
                    shapeTexture = TextureUtility.RotateTexture(shapeTexture, +45f);
                }

                modMaterial.SetTexture("_MainTex", shapeTexture);
            }

            modMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            return modMaterial;
        }

        private static ParticleSystem CreateParticleSystemFromTemplate(WidgetMinimap instance, Scene scene, Zone zone, ParticleSystem template, MarkerShape markerShape, string name)
        {
            // GameObject を複製
            var clonedGameObject = Object.Instantiate(template.gameObject);
            clonedGameObject.name = name;
            clonedGameObject.transform.SetParent(template.transform.parent, false);

            // transform をテンプレートに合わせる
            clonedGameObject.transform.localPosition = template.transform.localPosition;
            clonedGameObject.transform.localRotation = template.transform.localRotation;
            clonedGameObject.transform.localScale = template.transform.localScale;

            var modParticleSystem = clonedGameObject.GetComponent<ParticleSystem>();

            // main モジュールの調整
            var main = modParticleSystem.main;
            main.playOnAwake = false;
            main.startLifetime = instance.intervalPS;
            main.startSize = instance.psSize;

            // 自動エミッションをオフにして手動 Emit にする
            var emission = modParticleSystem.emission;
            emission.enabled = false;

            // レンダラのマテリアルは複製して上書き（オリジナルに影響を与えないため）
            var renderer = modParticleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            // 描画順を template の renderer.sortingOrder に合わせる(描画順序をあと勝ちにするために EmitParticlePlus で小細工)
            var templateRenderer = template.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerID = templateRenderer.sortingLayerID;
            renderer.sortingOrder = templateRenderer.sortingOrder;

            renderer.material = CreateMaterial(instance, scene, zone, renderer.sharedMaterial, markerShape);

            return modParticleSystem;
        }

        private static ParticleSystem BuildParticleSystemIfUnbuilt(WidgetMinimap instance, Scene scene, Zone zone, Marker marker)
        {
            if (marker.ParticleSystem != null)
            {
                return marker.ParticleSystem;
            }

            var name = $"{Package.Id}_{marker.GetType().Name}";
            var particleSystem = CreateParticleSystemFromTemplate(instance, scene, zone, instance.psAlly, marker.MarkerShape, name);

            return particleSystem;
        }

        private static void UpdateParticleSystem(WidgetMinimap instance, Scene scene, Zone zone, ParticleSystem particleSystem, MarkerShape markerShape)
        {
            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();

            var shared = renderer.sharedMaterial;
            var current = renderer.material;

            if (current != null && !ReferenceEquals(current, shared))
            {
                try
                {
                    Object.Destroy(current);
                }
                catch (System.Exception ex)
                {
                    ModHelper.LogNotExpected(ex);
                }
            }

            renderer.material = CreateMaterial(instance, scene, zone, shared, markerShape);
        }

        private static bool TryGetUpStair(Thing thing, MarkerFilter markerFilter, StairsMarkerSetting setting, out RefreshElement result)
        {
            if (setting.IsEnabled)
            {
                if (markerFilter.IsUpStair(thing, setting))
                {
                    result = new RefreshElement(thing, setting.UpColor);
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static bool TryGetDownStair(Thing thing, MarkerFilter markerFilter, StairsMarkerSetting setting, out RefreshElement result)
        {
            if (setting.IsEnabled)
            {
                if (markerFilter.IsDownStair(thing, setting))
                {
                    result = new RefreshElement(thing, setting.DownColor);
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static bool TryGetMinion(Chara character, Chara playerCharacter, MarkerFilter markerFilter, MarkerSetting setting, out RefreshElement result)
        {
            if (setting.IsEnabled)
            {
                if (markerFilter.IsMinion(character, playerCharacter, setting))
                {
                    result = new RefreshElement(character, setting.Color);
                    return true;
                }
            }
            result = default;
            return false;
        }

        private static bool TryGetPet(Chara character, Chara playerCharacter, MarkerFilter markerFilter, MarkerSetting setting, out RefreshElement result)
        {
            if (setting.IsEnabled)
            {
                if (markerFilter.IsPet(character, playerCharacter, setting))
                {
                    result = new RefreshElement(character, setting.Color);
                    return true;
                }
            }
            result = default;
            return false;
        }

        private static bool TryGetSpecialCharacter(Chara character, MarkerFilter markerFilter, SpecialCharacterMarkerSetting setting, out RefreshElement result)
        {
            if (setting.IsEnabled)
            {
                if (markerFilter.IsSpecialCharacter(character, setting))
                {
                    Color color;
                    switch (character)
                    {
                        case { c_bossType: BossType.Boss }:
                            color = setting.BossColor;
                            break;

                        case { c_bossType: BossType.Evolved }:
                            color = setting.EvolvedColor;
                            break;

                        case { id: ElinId.BigDaddyId }:
                            color = setting.BigDaddyColor;
                            break;

                        case { id: ElinId.SantaId }:
                            color = setting.SantaColor;
                            break;

                        default:
                            ModHelper.LogNotExpected([
                                "character",
                                $"character.id = {character.id}",
                                $"character.c_bossType = {character.c_bossType}",
                            ]);
                            color = new Color(0, 0, 0, 0);
                            break;
                    }

                    result = new RefreshElement(character, color);
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static bool TryGetSpecialThing(Thing thing, MarkerFilter markerFilter, SpecialThingMarkerSetting setting, out RefreshElement result)
        {
            if (setting.IsEnabled)
            {
                if (markerFilter.IsSpecialThing(thing, setting))
                {
                    Color color;
                    switch (thing.id)
                    {
                        case ElinId.StatuePower:
                            color = setting.ShrineColor;
                            break;

                        case ElinId.WaterJure:
                            color = setting.JureColor;
                            break;

                        default:
                            if (ElinId.SpecialThingGodOnlyIds.Contains(thing.id))
                            {
                                color = setting.GodColor;
                            }
                            else
                            {
                                ModHelper.LogNotExpected([
                                    "thing",
                                    $"thing.id = {thing.id}",
                                ]);
                                color = new Color(0, 0, 0, 0);
                            }
                            break;
                    }

                    result = new RefreshElement(thing, color);
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static bool TryGetCustomCharacter(Chara character, Chara playerCharacter, MarkerFilter markerFilter, CustomMarkerSetting setting, out RefreshElement result)
        {
            if (setting.IsEnabled)
            {
                if (markerFilter.IsCustomCharacterCandidate(character, playerCharacter, setting))
                {
                    if (!CustomHitCache.TryGetCharacter(character, out var isHit))
                    {
                        isHit = markerFilter.Judgement.IsCustomElement(character, setting.Character);
                        CustomHitCache.RegisterCharacter(character, isHit);
                    }
                    if (isHit)
                    {
                        result = new RefreshElement(character, setting.Character.Color);
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }

        private static bool TryGetCustomThing(Thing thing, MarkerFilter markerFilter, CustomMarkerSetting setting, out RefreshElement result)
        {
            if (setting.IsEnabled)
            {
                if (markerFilter.IsCustomThingCandidate(thing, setting))
                {
                    if (!CustomHitCache.TryGetThing(thing, out var isHit))
                    {
                        isHit = markerFilter.Judgement.IsCustomElement(thing, setting.Thing);
                        CustomHitCache.RegisterThing(thing, isHit);
                    }
                    if (isHit)
                    {
                        result = new RefreshElement(thing, setting.Thing.Color);
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }

        private static RefreshElements? CreateCustomRefreshElements(IReadOnlyCollection<RefreshElement> customCharacterItems, IReadOnlyCollection<RefreshElement> customThingItems, CustomMarkerSetting setting)
        {
            var customItemsCount = customCharacterItems.Count + customThingItems.Count;

            if (customItemsCount == 0)
            {
                return null;
            }

            IReadOnlyCollection<RefreshElement>[] items = setting.PrioritizeThing
                ? [customCharacterItems, customThingItems]
                : [customThingItems, customCharacterItems]
            ;
            var elements = new List<RefreshElement>(customItemsCount);
            foreach (var collection in items)
            {
                elements.AddRange(collection);
            }
            var refreshElements = new RefreshElements(CustomMarker, elements);
            return refreshElements;
        }

        private static List<RefreshElements> GetRefreshTargets(Chara playerCharacter, IEnumerable<Chara> characters, IEnumerable<Thing> things, MarkerFilter markerFilter, Setting setting)
        {
            var stairUpRefreshElements = new RefreshElements(StairUpMarker);
            var stairDownRefreshElements = new RefreshElements(StairDownMarker);
            var minionRefreshElements = new RefreshElements(MinionMarker);
            var petRefreshElements = new RefreshElements(PetMarker);
            var specialCharacterRefreshElements = new RefreshElements(SpecialCharacterMarker);
            var specialThingRefreshElements = new RefreshElements(SpecialThingMarker);
            // カスタムマーカーは後でごちゃごちゃするのでリストだけ構築
            var customCharacterItems = new List<RefreshElement>();
            var customThingItems = new List<RefreshElement>();

            var refreshMarkers = new List<RefreshElements>(Markers.Count)
            {
                stairUpRefreshElements,
                stairDownRefreshElements,
                minionRefreshElements,
                petRefreshElements,
                specialCharacterRefreshElements,
                specialThingRefreshElements,
            };

            // キャラクター列挙
            foreach (var character in characters)
            {
                // ミニオン
                if (TryGetMinion(character, playerCharacter, markerFilter, setting.Minion, out var minionResult))
                {
                    minionRefreshElements.Add(minionResult);
                }

                // ペット
                if (TryGetPet(character, playerCharacter, markerFilter, setting.Pet, out var petResult))
                {
                    petRefreshElements.Add(petResult);
                }

                // 特別キャラクター
                if (TryGetSpecialCharacter(character, markerFilter, setting.SpecialCharacter, out var specialCharacterResult))
                {
                    specialCharacterRefreshElements.Add(specialCharacterResult);
                }

                // カスタムキャラクター
                if (TryGetCustomCharacter(character, playerCharacter, markerFilter, setting.Custom, out var customCharacterResult))
                {
                    customCharacterItems.Add(customCharacterResult);
                }
            }

            // アイテム列挙
            foreach (var thing in things)
            {
                // 上り階段
                if (TryGetUpStair(thing, markerFilter, setting.Stairs, out var upStairResult))
                {
                    stairUpRefreshElements.Add(upStairResult);
                }

                // 下り階段
                if (TryGetDownStair(thing, markerFilter, setting.Stairs, out var downStairResult))
                {
                    stairDownRefreshElements.Add(downStairResult);
                }

                // 特別アイテム
                if (TryGetSpecialThing(thing, markerFilter, setting.SpecialThing, out var specialThingResult))
                {
                    specialThingRefreshElements.Add(specialThingResult);
                }

                // カスタムアイテム
                if (TryGetCustomThing(thing, markerFilter, setting.Custom, out var customThingResult))
                {
                    customThingItems.Add(customThingResult);
                }
            }

            var customRefreshElements = CreateCustomRefreshElements(customCharacterItems, customThingItems, setting.Custom);
            if (customRefreshElements is not null)
            {
                refreshMarkers.Add(customRefreshElements);
            }


            return refreshMarkers;
        }

        private static void InvokeReloadIfPossible(WidgetMinimap instance, Scene scene, Zone zone, MapRefreshSetting mapRefreshSetting)
        {
            instance.CancelInvoke(nameof(instance.Reload));

            if (!mapRefreshSetting.IsEnabled)
            {
                ModHelper.WriteDebug("設定により抑制");
                return;
            }

            if (ModHelper.Elin.IsGlobalMap(scene, zone))
            {
                ModHelper.WriteDebug("グローバル無視");
                return;
            }

            var breakpointSetting = mapRefreshSetting.Breakpoints
                .GetSortedItems()
                .FirstOrDefault(b => instance.Size <= b.Width)
            ;
            var interval = breakpointSetting?.Interval ?? mapRefreshSetting.FallbackInterval;

            ModHelper.LogDebug($"{nameof(instance.InvokeRepeating)} {nameof(instance.Reload)}: {nameof(interval)}: {interval}, {nameof(instance.Size)}: {instance.Size}");
            instance.InvokeRepeating(
                nameof(instance.Reload),
                2, // 初期待機時間は少しだけ待つ(相手は人間なので即時でなんかするとも思えない)
                (float)interval.TotalSeconds
            );
        }

        private static void BuildCustomHitCache(WidgetMinimap instance, Scene scene, Zone zone, Chara playerCharacter, IEnumerable<Chara> characters, IEnumerable<Thing> things, Judgement judgement, CustomMarkerSetting customMarkerSetting)
        {
            CustomHitCache.Clear();

            if (!customMarkerSetting.IsEnabled)
            {
                return;
            }

            // 自拠点無視
            if (customMarkerSetting.IgnoreSelfZone && ModHelper.Elin.IsSelfZone(zone))
            {
                return;
            }

            // クソ重太郎(ここでキャッシュ構築すればフレーム毎の重さは解消されるのでヨシ)
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var character in characters)
            {
                if (!judgement.IsCustomCharacterCandidate(character, playerCharacter))
                {
                    // ペットになったり外されたりするのでキャッシュ構築では使用しない
                    continue;
                }

                if (!judgement.IsSeen(character.Cell, customMarkerSetting.ShowEvenIfUnseen))
                {
                    // 視認していない場合、キャッシュしない
                    continue;
                }

                if (judgement.IsCustomElement(character, customMarkerSetting.Character))
                {
                    CustomHitCache.RegisterCharacter(character, true);
                }
            }

            foreach (var thing in things)
            {
                if (!judgement.IsCustomThingCandidate(thing))
                {
                    // アイテムは変わらないと思うのキャッシュする
                    CustomHitCache.RegisterThing(thing, false);
                    continue;
                }

                if (!judgement.IsSeen(thing.Cell, customMarkerSetting.ShowEvenIfUnseen))
                {
                    continue;
                }

                if (judgement.IsCustomElement(thing, customMarkerSetting.Thing))
                {
                    CustomHitCache.RegisterThing(thing, true);
                }
            }

#if DEBUG
            ModHelper.WriteDebug($"<Build> {stopwatch.ElapsedMilliseconds} ms");
#else
            ModHelper.Logger.LogInfo($"<Build> {stopwatch.ElapsedMilliseconds} ms");
#endif
        }

        private static void Reload(WidgetMinimap instance, Scene scene, Zone zone, Chara playerCharacter, IEnumerable<Chara> characters, IEnumerable<Thing> things, Judgement judgement, Setting setting)
        {
            InvokeReloadIfPossible(instance, scene, zone, setting.MapRefresh);
            BuildCustomHitCache(instance, scene, zone, playerCharacter, characters, things, judgement, setting.Custom);
        }

        private static List<Cell> GetAroundCells(WidgetMinimap instance, Cell[,] cells, Point2D center, PointerOverSetting pointerOverSetting)
        {
            var breakpointSetting = pointerOverSetting.Breakpoints
                .GetSortedItems()
                .FirstOrDefault(b => instance.Size <= b.Width)
            ;
            var aroundCount = breakpointSetting?.AroundCount ?? pointerOverSetting.FallbackAroundCount;

            var min = new Point2D(center.X - aroundCount, center.Y - aroundCount);
            var max = new Point2D(center.X + aroundCount, center.Y + aroundCount);
            var result = new List<Cell>((aroundCount + 1) * 2);
            var width = cells.GetLength(0);
            var height = cells.GetLength(1);
            for (var y = min.Y; y <= max.Y; y++)
            {
                if (0 <= y && y < height)
                {
                    for (var x = min.X; x <= max.X; x++)
                    {
                        if (0 <= x && x < width)
                        {
                            var cell = cells[x, y];
                            result.Add(cell);
                        }
                    }
                }
            }

            return result;
        }

        private static void SetTooltipContent(TooltipData tooltipData, MinimapTooltip minimapTooltip, Setting setting, Point2D center)
        {
            tooltipData.onShowTooltip = (t) =>
            {
                t.note.Clear();

                if (0 < minimapTooltip.Pets.Count)
                {
                    t.note.AddHeader(ModHelper.Lang.General.NearbyPets);
                    var pets = minimapTooltip.GetPets(setting.PointerOver.MaxShownPerGroup, center);
                    foreach (var pet in pets.Displayed)
                    {
                        var uiItem = t.note.AddHeaderCard(pet.Name);
                        pet.SetImage(uiItem.image2);
                        //TODO: "HP" ってローカライズあり？
                        uiItem.text2.SetText($"HP: {pet.hp:N0}/{pet.MaxHP:N0}");
                    }
                    if (0 < pets.Hidden.Count)
                    {
                        t.note.AddText(ModHelper.Lang.Formatter.FormatHiddenByLimit(count: pets.Hidden.Count), FontColor.Gray);
                    }
                }

                if (0 < minimapTooltip.SpecialCharacters.Count)
                {
                    t.note.AddHeader(ModHelper.Lang.General.NearbySpecialCharacters);
                    var characters = minimapTooltip.GetSpecialCharacters(setting.PointerOver.MaxShownPerGroup, center);
                    foreach (var character in characters.Displayed)
                    {
                        var uiItem = t.note.AddHeaderCard(character.Name);
                        character.SetImage(uiItem.image2);

                        // [ELIN:Chara.GetHoverText]
                        // -> string s = (IsFriendOrAbove() ? "HostilityAlly" : (IsNeutral() ? "HostilityNeutral" : "HostilityEnemy"));
                        string s = (character.IsFriendOrAbove() ? "HostilityAlly" : (character.IsNeutral() ? "HostilityNeutral" : "HostilityEnemy"));
                        var color = s switch
                        {
                            "HostilityAlly" => FontColor.Good, // 友好
                            "HostilityNeutral" => FontColor.Default, // 中立
                            "HostilityEnemy" => FontColor.Bad, // 敵対
                            _ => FontColor.Warning,
                        };
                        if (color == FontColor.Warning)
                        {
                            ModHelper.LogNotExpected([
                                "character",
                                $"character.id = {character.id}",
                                $"character.c_bossType = {character.c_bossType}",
                                $"character.IsFriendOrAbove() = {character.IsFriendOrAbove()}",
                                $"character.IsNeutral() = {character.IsNeutral()}",
                            ]);
                        }

                        uiItem.text2.SetText(s.lang(), color);
                    }
                    if (0 < characters.Hidden.Count)
                    {
                        t.note.AddText(ModHelper.Lang.Formatter.FormatHiddenByLimit(count: characters.Hidden.Count), FontColor.Gray);
                    }
                }

                if (0 < minimapTooltip.SpecialThings.Count)
                {
                    t.note.AddHeader(ModHelper.Lang.General.NearbySpecialThing);
                    var things = minimapTooltip.GetSpecialThings(setting.PointerOver.MaxShownPerGroup, center);
                    foreach (var thing in things.Displayed)
                    {
                        var uiItem = t.note.AddHeaderCard(thing.Name);
                        thing.SetImage(uiItem.image2);
                    }
                    if (0 < things.Hidden.Count)
                    {
                        t.note.AddText(ModHelper.Lang.Formatter.FormatHiddenByLimit(count: things.Hidden.Count), FontColor.Gray);
                    }
                }

                if (0 < minimapTooltip.CustomCharacters.Count)
                {
                    t.note.AddHeader(ModHelper.Lang.General.NearbyCustomCharacters);
                    var characters = minimapTooltip.GetCustomCharacters(setting.PointerOver.MaxShownPerGroup, center);
                    foreach (var character in characters.Displayed)
                    {
                        var uiItem = t.note.AddHeaderCard(character.Name);
                        character.SetImage(uiItem.image2);
                    }
                    if (0 < characters.Hidden.Count)
                    {
                        t.note.AddText(ModHelper.Lang.Formatter.FormatHiddenByLimit(count: characters.Hidden.Count), FontColor.Gray);
                    }
                }

                if (0 < minimapTooltip.CustomThings.Count)
                {
                    t.note.AddHeader(ModHelper.Lang.General.NearbyCustomItems);
                    var things = minimapTooltip.GetCustomItems(setting.PointerOver.MaxShownPerGroup, center);
                    foreach (var thing in things.Displayed)
                    {
                        var uiItem = t.note.AddHeaderCard(thing.Name);
                        thing.SetImage(uiItem.image2);
                    }
                    if (0 < things.Hidden.Count)
                    {
                        t.note.AddText(ModHelper.Lang.Formatter.FormatHiddenByLimit(count: things.Hidden.Count), FontColor.Gray);
                    }
                }

                t.note.Build();
            };
        }

        private static bool ShowCellsNote(WidgetMinimap instance, Zone zone, Chara playerCharacter, Point2D center, IReadOnlyCollection<Cell> cells, Setting setting)
        {
            Debug.Assert(setting.PointerOver.IsEnabled);

            // MarkerFilter を使うと処理が共通化できるが、判定順序の問題で無駄な処理が発生するので直接処理している、、、が、別に良くないか
            var judgement = new Judgement();

            // linq 重いか？ ベンチ取ってないからわからん

            // 階段・ミニオンはツールチップ表示してもあんまり嬉しくない気がするので対応しない

            // ペット
            var pets = !setting.Pet.IsEnabled
                ? []
                : cells
                    .SelectMany(
                        cell => FilterSearchableCharacters(cell.Charas, judgement)
                            .Where(a => judgement.IsPet(a, playerCharacter))
                    )
                    .ToArray()
            ;

            // 特別キャラクター
            var specialCharacters = !setting.SpecialCharacter.IsEnabled || ModHelper.Elin.IsDefenseGame(zone)
                ? []
                : cells
                    .Where(cell => judgement.IsSeen(cell, setting.SpecialCharacter.ShowEvenIfUnseen))
                    .SelectMany(
                        cell => FilterSearchableCharacters(cell.Charas, judgement)
                            .Where(a => judgement.IsSpecialCharacter(a))
                    )
                    .ToArray()
            ;

            // 特別アイテム
            var specialThing = !setting.SpecialThing.IsEnabled || (setting.SpecialThing.IgnoreSelfZone && ModHelper.Elin.IsSelfZone(zone))
                ? []
                : cells
                    .Where(cell => judgement.IsSeen(cell, setting.SpecialThing.ShowEvenIfUnseen))
                    .SelectMany(
                        cell => FilterSearchableThings(cell.Things, judgement)
                            .Where(a => judgement.IsSpecialThing(a))
                    )
                    .ToArray()
            ;

            // カスタムキャラクター
            var customCharacters = !setting.Custom.IsEnabled || (setting.Custom.IgnoreSelfZone && ModHelper.Elin.IsSelfZone(zone))
                ? []
                : cells
                    .Where(cell => judgement.IsSeen(cell, setting.Custom.ShowEvenIfUnseen))
                    .SelectMany(
                        cell => FilterSearchableCharacters(cell.Charas, judgement)
                            .Where(a => judgement.IsCustomCharacterCandidate(a, playerCharacter))
                            .Where(a => judgement.IsCustomElement(a, setting.Custom.Character))
                    )
                    .ToArray()
            ;

            // カスタムアイテム
            var customThings = !setting.Custom.IsEnabled || (setting.Custom.IgnoreSelfZone && ModHelper.Elin.IsSelfZone(zone))
                ? []
                : cells
                    .Where(cell => judgement.IsSeen(cell, setting.Custom.ShowEvenIfUnseen))
                    .SelectMany(
                        cell => FilterSearchableThings(cell.Things, judgement)
                            .Where(a => judgement.IsCustomThingCandidate(a))
                            .Where(a => judgement.IsCustomElement(a, setting.Custom.Thing))
                    )
                    .ToArray()
            ;

            var minimapTooltip = new MinimapTooltip(pets, specialCharacters, specialThing, customCharacters, customThings);
            if (!minimapTooltip.IsShowable)
            {
                return false;
            }

            SetTooltipContent(TooltipData, minimapTooltip, setting, center);


            // 色々見よう見まねで試してみたけど座標系が全然わからん、、、
            // マップウィジェットに重ねたくないだけなんよ
            // 一旦あきらめ

            var size = instance.rectMap.rect.size;

            // 横幅ずらすが、回転している場合はその分も考慮して対角線の長さくらいにする(横幅自体を拡縮できるから微妙感はある)
            var width = size.x;
            if (instance.extra.rotate && !zone.IsRegion)
            {
                width = (size.x + size.y) / Mathf.Sqrt(2);
            }

            TooltipData.offset = new Vector3(width / 2 * 1.2f, size.y / 2);
            TooltipData.enable = true;
            TooltipManager.Instance.ShowTooltip(TooltipData, instance.transform);
            return true;
        }

        internal static void Rebuild(WidgetMinimap instance, Setting setting)
        {
            // うおおおおおおお！めちゃくちゃ！！

            var judgement = new Judgement();
            var characters = FilterSearchableCharacters(EMono._map.charas, judgement);
            var things = FilterSearchableThings(EMono._map.things, judgement);

            BuildCustomHitCache(instance, EMono.scene, EMono._zone, EMono.pc, characters, things, judgement, setting.Custom);
        }

        #endregion

        #region WidgetMinimap

        /// <summary>
        /// <see cref="WidgetMinimap.EmitParticle"/> 独自描画用。
        /// </summary>
        /// <remarks>[ELIN:WidgetMinimap.EmitParticle]</remarks>
        /// <seealso cref="WidgetMinimap.EmitParticle"/>
        /// <param name="instance"></param>
        /// <param name="c"></param>
        /// <param name="ps"></param>
        /// <param name="col"></param>
        /// <param name="particleSystem">独自追加</param>
        /// <param name="scale"></param>
        private static void EmitParticlePlus(WidgetMinimap instance, Card c, ParticleSystem ps, Color col, ParticleSystem particleSystem, float scale)
        {
            int num = c.pos.x;
            int num2 = c.pos.z;
            float z = (c.IsPCFactionOrMinion ? 9f : (10f + 0.01f * (float)(c.pos.z * 200 + c.pos.x)));
            if (instance.preview.limitBounds)
            {
                num -= EMono._map.bounds.x - instance.preview.offsetX;
                num2 -= EMono._map.bounds.z - instance.preview.offsetZ;
            }
            float x = (float)num / (float)instance.Size - 0.5f;
            float y = (float)num2 / (float)instance.Size - 0.5f;
            //count++;

            // 無理やりなあと描画勝利
            const float EmitZStep = 0.001f;
            float zAdjusted = z - (emitSequence++ * EmitZStep);

            ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
            emitParams.position = new Vector3(x, y, zAdjusted);
            emitParams.startSize = instance.psSize * scale;
            emitParams.startLifetime = instance.intervalPS;
            emitParams.startColor = col;

            particleSystem.Emit(emitParams, 1);
        }

        public static void OnActivatePrefix(WidgetMinimap instance)
        {
            Reset(instance);
        }

        internal static void OnActivatePostfix(WidgetMinimap instance, Setting setting)
        {
            var judgement = new Judgement();

            var characters = FilterSearchableCharacters(EMono._map.charas, judgement);
            var things = FilterSearchableThings(EMono._map.things, judgement);

            Reload(instance, EMono.scene, EMono._zone, EMono.pc, characters, things, judgement, setting);
        }

        public static void OnMoveZonePostfix(WidgetMinimap instance, Setting setting)
        {
            // OnActivatePostfix とやってることは同じだが、呼び出しの起点が異なるため処理の差異も出てくる可能性があり、分けて記述（ないだろうけど）
            var judgement = new Judgement();
            var characters = FilterSearchableCharacters(EMono._map.charas, judgement);
            var things = FilterSearchableThings(EMono._map.things, judgement);

            Reload(instance, EMono.scene, EMono._zone, EMono.pc, characters, things, judgement, setting);
        }

        public static void OnUpdatePostfix(WidgetMinimap instance, Setting setting)
        {
            if (!setting.PointerOver.IsEnabled)
            {
                return;
            }

            void Bye()
            {
                if (TooltipData.enable)
                {
                    TooltipManager.Instance.HideTooltips();
                }
                TooltipData.enable = false;
            }

            // [ELIN:WidgetMinimap.OnPointerDown]
            // -> if (!EMono.pc.HasNoGoal || EMono.game.activeZone.IsRegion || EMono.ui.IsActive)
            if (!EMono.pc.HasNoGoal || EMono.game.activeZone.IsRegion || EMono.ui.IsActive)
            {
                Bye();
                return;
            }

            if (!TryGetMinimapPointToCellPosition(instance, Input.mousePosition, ELayer.ui.canvas.worldCamera, out var position))
            {
                Bye();
                return;
            }

            var shown = false;
            var cells = GetAroundCells(instance, EMono._map.cells, position, setting.PointerOver);
            if (0 < cells.Count)
            {
                shown = ShowCellsNote(instance, EClass._zone, EMono.pc, position, cells, setting);
            }

            if (!shown)
            {
                Bye();
            }
        }

        public static bool OnPointerDownPrefix(WidgetMinimap instance, PointerEventData e, Setting setting)
        {
            if (!setting.AllowMoveWhenUnseen)
            {
                return true;
            }

            // [ELIN:WidgetMinimap.OnPointerDown]
            // -> if (!EMono.pc.HasNoGoal || EMono.game.activeZone.IsRegion || EMono.ui.IsActive)
            if (!EMono.pc.HasNoGoal || EMono.game.activeZone.IsRegion || EMono.ui.IsActive)
            {
                return true;
            }

            // ミニマップのクリックから強制移動
            // 極力本体処理を使用したいため移動が必要なければ再計算することになっても本体に処理をまわす
            if (!TryGetMinimapPointToCellPosition(instance, e.position, e.pressEventCamera, out var position))
            {
                return true;
            }

            var cell = EMono._map.cells[position.X, position.Y];
            if (cell.isSeen)
            {
                return true;
            }

            ModHelper.LogDebug("マップクリック検知からの強制移動");

            // [ELIN:WidgetMinimap.OnPointerDown]
            // -> EMono.pc.SetAIImmediate(new AI_Goto(pos, 0));
            var pos = new Point();
            pos.Set(position.X, position.Y);
            EMono.pc.SetAI(new AI_Goto(pos, 0));
            return false;
        }

        public static void OnSetContextMenuPrefix(WidgetMinimap instance, UIContextMenu m, Setting setting)
        {
            var minimapMenu = new MinimapMenu(instance, m, setting);

            minimapMenu.Apply();
        }

        public static void RefreshMarkersPostfix(WidgetMinimap instance, Setting setting)
        {
            if (!instance.gameObject.activeInHierarchy)
            {
                return;
            }
            // グローバルマップとかは対象外(で、いいのか？ 死亡位置とか必要？)
            if (ModHelper.Elin.IsGlobalMap(EMono.scene, EMono._zone))
            {
                return;
            }

            // あと勝ちに任せて描画最適化は行わない

            // フレームごとの Emit 順カウンタをリセット（この呼び出し内の Emit 順が描画順になる）
            emitSequence = 0;

            var judgement = new Judgement();
            var markerFilter = new MarkerFilter(judgement, EMono._zone);

            var characters = FilterSearchableCharacters(EMono._map.charas, judgement);
            var things = FilterSearchableThings(EMono._map.things, judgement);

            var refreshMarkers = GetRefreshTargets(EMono.pc, characters, things, markerFilter, setting);
            foreach (var refreshMarker in refreshMarkers)
            {
                if (refreshMarker.Marker.ParticleSystem == null && refreshMarker.Elements.Count == 0)
                {
                    continue;
                }

                refreshMarker.Marker.ParticleSystem = BuildParticleSystemIfUnbuilt(instance, EMono.scene, EMono._zone, refreshMarker.Marker);
                refreshMarker.Marker.ParticleSystem.Clear();

                foreach (var element in refreshMarker.Elements)
                {
                    EmitParticlePlus(instance, element.Target, instance.psAlly, element.Color, refreshMarker.Marker.ParticleSystem, BillboardSizeScale);
                }
            }

            // PC再描画
            if (setting.IsForegroundPlayer)
            {
                // あんまうまくいってない感あったけどやけくそで適当に一旦逃げる
                emitSequence *= 2;
                // [ELIN:WidgetMinimap.RefreshMarkers]
                // -> EmitParticle(EMono.pc, psAlly, colorAlly)
                EmitParticlePlus(instance, EMono.pc, instance.psAlly, instance.colorAlly, instance.psAlly, 1);
            }
        }

        public static void RefreshStylePostfix(WidgetMinimap instance, Setting setting)
        {
            if (!instance.gameObject.activeInHierarchy)
            {
                return;
            }

            foreach (var marker in Markers)
            {
                if (marker.ParticleSystem != null)
                {
                    UpdateParticleSystem(instance, EMono.scene, EMono._zone, marker.ParticleSystem, marker.MarkerShape);
                }
            }
        }


        #endregion
    }
}

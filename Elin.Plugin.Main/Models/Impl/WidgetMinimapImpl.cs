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
                    marker.ParticleSystem = null;
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

        private static void RefreshStairMarkers(WidgetMinimap instance, Scene scene, Zone zone, Chara playerCharacter, IEnumerable<Thing> things, Judgement judgement, StairsMarkerSetting stairsSetting)
        {
            Debug.Assert(stairsSetting.IsEnabled);

            var particleCleanerUp = new ParticleCleaner();
            var particleCleanerDown = new ParticleCleaner();

            foreach (var thing in things)
            {
                if (!judgement.IsStair(thing))
                {
                    continue;
                }

                Debug.Assert(thing.trait is TraitStairs);

                if (!judgement.IsSeen(thing.Cell, stairsSetting.ShowEvenIfUnseen))
                {
                    continue;
                }

                var isUp = thing.trait is TraitStairsUp;

                if (isUp)
                {
                    StairUpMarker.ParticleSystem = BuildParticleSystemIfUnbuilt(instance, scene, zone, StairUpMarker);
                }
                else
                {
                    StairDownMarker.ParticleSystem = BuildParticleSystemIfUnbuilt(instance, scene, zone, StairDownMarker);
                }

                if (isUp)
                {
                    Debug.Assert(StairUpMarker.ParticleSystem is not null);
                    particleCleanerUp.ClearIfNotCleaned(StairUpMarker.ParticleSystem!);

                    EmitParticlePlus(instance, thing, instance.psAlly, stairsSetting.UpColor, StairUpMarker.ParticleSystem!, BillboardSizeScale);
                }
                else
                {
                    Debug.Assert(StairDownMarker.ParticleSystem is not null);
                    particleCleanerDown.ClearIfNotCleaned(StairDownMarker.ParticleSystem!);

                    EmitParticlePlus(instance, thing, instance.psAlly, stairsSetting.DownColor, StairDownMarker.ParticleSystem!, BillboardSizeScale);
                }
            }
        }

        private static void RefreshMinionMarkers(WidgetMinimap instance, Scene scene, Zone zone, Chara playerCharacter, IEnumerable<Chara> characters, Judgement judgement, MarkerSetting minionSetting)
        {
            Debug.Assert(minionSetting.IsEnabled);

            var particleCleaner = new ParticleCleaner();
            foreach (var character in characters)
            {
                if (!judgement.IsMinion(character, playerCharacter))
                {
                    continue;
                }

                MinionMarker.ParticleSystem = BuildParticleSystemIfUnbuilt(instance, scene, zone, MinionMarker);
                particleCleaner.ClearIfNotCleaned(MinionMarker.ParticleSystem);

                EmitParticlePlus(instance, character, instance.psAlly, minionSetting.Color, MinionMarker.ParticleSystem, BillboardSizeScale);
            }
        }

        private static void RefreshPetMarkers(WidgetMinimap instance, Scene scene, Zone zone, Chara playerCharacter, IEnumerable<Chara> characters, Judgement judgement, MarkerSetting petSetting)
        {
            Debug.Assert(petSetting.IsEnabled);

            var particleCleaner = new ParticleCleaner();
            foreach (var character in characters)
            {
                if (!judgement.IsPet(character, playerCharacter))
                {
                    continue;
                }

                PetMarker.ParticleSystem = BuildParticleSystemIfUnbuilt(instance, scene, zone, PetMarker);
                particleCleaner.ClearIfNotCleaned(PetMarker.ParticleSystem);

                EmitParticlePlus(instance, character, instance.psAlly, petSetting.Color, PetMarker.ParticleSystem, BillboardSizeScale);
            }
        }

        private static void RefreshSpecialCharacterMarkers(WidgetMinimap instance, Scene scene, Zone zone, Chara playerCharacter, IEnumerable<Chara> characters, Judgement judgement, SpecialCharacterMarkerSetting specialCharacterSetting)
        {
            Debug.Assert(specialCharacterSetting.IsEnabled);

            // 戦争中は既存のマーカーと衝突するので無視
            if (ModHelper.Elin.IsDefenseGame(zone))
            {
                return;
            }

            var particleCleaner = new ParticleCleaner();
            foreach (var character in characters)
            {
                if (!judgement.IsSpecialCharacter(character))
                {
                    continue;
                }

                if (!judgement.IsSeen(character.Cell, specialCharacterSetting.ShowEvenIfUnseen))
                {
                    continue;
                }

                SpecialCharacterMarker.ParticleSystem = BuildParticleSystemIfUnbuilt(instance, scene, zone, SpecialCharacterMarker);
                particleCleaner.ClearIfNotCleaned(SpecialCharacterMarker.ParticleSystem);

                Color color;
                switch (character)
                {
                    case { c_bossType: BossType.Boss }:
                        color = specialCharacterSetting.BossColor;
                        break;

                    case { c_bossType: BossType.Evolved }:
                        color = specialCharacterSetting.EvolvedColor;
                        break;

                    case { id: ElinId.BigDaddyId }:
                        color = specialCharacterSetting.BigDaddyColor;
                        break;

                    case { id: ElinId.SantaId }:
                        color = specialCharacterSetting.SantaColor;
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

                EmitParticlePlus(instance, character, instance.psAlly, color, SpecialCharacterMarker.ParticleSystem, BillboardSizeScale);
            }
        }

        private static void RefreshSpecialThingMarkers(WidgetMinimap instance, Scene scene, Zone zone, Chara playerCharacter, IEnumerable<Thing> things, Judgement judgement, SpecialThingMarkerSetting specialThingSetting)
        {
            Debug.Assert(specialThingSetting.IsEnabled);

            // 自拠点無視
            if (specialThingSetting.IgnoreSelfZone && ModHelper.Elin.IsSlefZone(zone))
            {
                return;
            }

            var particleCleaner = new ParticleCleaner();
            foreach (var thing in things)
            {
                if (!judgement.IsSpecialThing(thing))
                {
                    continue;
                }

                if (!judgement.IsSeen(thing.Cell, specialThingSetting.ShowEvenIfUnseen))
                {
                    continue;
                }

                SpecialThingMarker.ParticleSystem = BuildParticleSystemIfUnbuilt(instance, scene, zone, SpecialThingMarker);
                particleCleaner.ClearIfNotCleaned(SpecialThingMarker.ParticleSystem);

                Color color;
                switch (thing.id)
                {
                    case ElinId.StatuePower:
                        color = specialThingSetting.ShrineColor;
                        break;

                    case ElinId.WaterJure:
                        color = specialThingSetting.JureColor;
                        break;

                    default:
                        if (ElinId.SpecialThingGodOnlyIds.Contains(thing.id))
                        {
                            color = specialThingSetting.GodColor;
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

                EmitParticlePlus(instance, thing, instance.psAlly, color, SpecialThingMarker.ParticleSystem, BillboardSizeScale);
            }
        }

        private static void RefreshCustomMarkers(WidgetMinimap instance, Scene scene, Zone zone, Chara playerCharacter, IEnumerable<Chara> characters, IEnumerable<Thing> things, Judgement judgement, CustomMarkerSetting customMarkerSetting)
        {
            Debug.Assert(customMarkerSetting.IsEnabled);

            // 自拠点無視
            if (customMarkerSetting.IgnoreSelfZone && ModHelper.Elin.IsSlefZone(zone))
            {
                return;
            }

            var stockThings = new List<Thing>();
            var stockCharacters = new List<Chara>();

            //var s = System.Diagnostics.Stopwatch.StartNew();

            foreach (var character in characters)
            {
                if (!judgement.IsCustomCharacterCandidate(character, playerCharacter))
                {
                    continue;
                }

                if (!judgement.IsSeen(character.Cell, customMarkerSetting.ShowEvenIfUnseen))
                {
                    continue;
                }

                if (!CustomHitCache.TryGetCharacter(character, out var isHit))
                {
                    isHit = judgement.IsCustomElement(character, customMarkerSetting.Character);
                    CustomHitCache.RegisterCharacter(character, isHit);
                }
                if (isHit)
                {
                    stockCharacters.Add(character);
                }
            }
            foreach (var thing in things)
            {
                if (!judgement.IsCustomThingCandidate(thing))
                {
                    continue;
                }

                if (!judgement.IsSeen(thing.Cell, customMarkerSetting.ShowEvenIfUnseen))
                {
                    continue;
                }

                if (!CustomHitCache.TryGetThing(thing, out var isHit))
                {
                    isHit = judgement.IsCustomElement(thing, customMarkerSetting.Thing);
                    CustomHitCache.RegisterThing(thing, isHit);
                }
                if (isHit)
                {
                    stockThings.Add(thing);
                }
            }

            //ModHelper.WriteDebug($"[Refresh] {s.ElapsedMilliseconds} ms");

            // なんなんだこれは
            // あと勝ちなので優先する方をあとに指定
            IReadOnlyCollection<Card>[] stockItems = customMarkerSetting.PrioritizeThing
                ? [stockCharacters, stockThings]
                : [stockThings, stockCharacters]
            ;
            var colors = new Queue<Color>(
                customMarkerSetting.PrioritizeThing
                    ? [customMarkerSetting.Character.Color, customMarkerSetting.Thing.Color]
                    : [customMarkerSetting.Thing.Color, customMarkerSetting.Character.Color]
            );

            var particleCleaner = new ParticleCleaner();
            foreach (var stockItemList in stockItems)
            {
                var color = colors.Dequeue();
                foreach (var item in stockItemList)
                {
                    CustomMarker.ParticleSystem = BuildParticleSystemIfUnbuilt(instance, scene, zone, CustomMarker);
                    particleCleaner.ClearIfNotCleaned(CustomMarker.ParticleSystem);
                    EmitParticlePlus(instance, item, instance.psAlly, color, CustomMarker.ParticleSystem, BillboardSizeScale);
                }
            }
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
            if (customMarkerSetting.IgnoreSelfZone && ModHelper.Elin.IsSlefZone(zone))
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
                if (0 < y && y < height)
                {
                    for (var x = min.X; x <= max.X; x++)
                    {
                        if (0 < x && x < width)
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
                        if (0 < characters.Hidden.Count)
                        {
                            t.note.AddText(ModHelper.Lang.Formatter.FormatHiddenByLimit(count: characters.Hidden.Count), FontColor.Gray);
                        }
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

            var judgement = new Judgement();

            // linq 重いか？ ベンチ取ってないからわからん

            // 階段・ミニオンはツールチップ表示してもあんまり嬉しくない気がするので対応しない

            // ペット
            var pets = !setting.Pet.IsEnabled
                ? []
                : cells
                    .SelectMany(
                        cell => FilterSearchableCharacters(cell.Charas, judgement)
                            .Where(a => !a.IsPC && judgement.IsPet(a, playerCharacter))
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
                            .Where(a => !a.IsPC && judgement.IsSpecialCharacter(a))
                    )
                    .ToArray()
            ;

            // 特別アイテム
            var specialThing = !setting.SpecialThing.IsEnabled || (setting.SpecialThing.IgnoreSelfZone && ModHelper.Elin.IsSlefZone(zone))
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
            var customCharacters = !setting.Custom.IsEnabled || (setting.Custom.IgnoreSelfZone && ModHelper.Elin.IsSlefZone(zone))
                ? []
                : cells
                    .Where(cell => judgement.IsSeen(cell, setting.Custom.ShowEvenIfUnseen))
                    .SelectMany(
                        cell => FilterSearchableCharacters(cell.Charas, judgement)
                            .Where(a => !a.IsPC && judgement.IsCustomCharacterCandidate(a, playerCharacter))
                            .Where(a => judgement.IsCustomElement(a, setting.Custom.Character))
                    )
                    .ToArray()
            ;

            // カスタムアイテム
            var customThings = !setting.Custom.IsEnabled || (setting.Custom.IgnoreSelfZone && ModHelper.Elin.IsSlefZone(zone))
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
            // 列挙したデータは使用するかこの段階では分からないため実体化しない
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

        public static void OnSetContextMenuPostfix(WidgetMinimap instance, UIContextMenu m, Setting setting)
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

            var characters = FilterSearchableCharacters(EMono._map.charas, judgement)
                .ToArray()
            ;
            var things = FilterSearchableThings(EMono._map.things, judgement)
                .ToArray()
            ;

            // 階段描画
            if (setting.Stairs.IsEnabled)
            {
                RefreshStairMarkers(instance, EMono.scene, EMono._zone, EMono.pc, things, judgement, setting.Stairs);
            }

            // ミニオン描画
            if (setting.Minion.IsEnabled)
            {
                RefreshMinionMarkers(instance, EMono.scene, EMono._zone, EMono.pc, characters, judgement, setting.Minion);
            }

            // ペット描画
            if (setting.Pet.IsEnabled)
            {
                RefreshPetMarkers(instance, EMono.scene, EMono._zone, EMono.pc, characters, judgement, setting.Pet);
            }

            // 特殊キャラクター描画
            if (setting.SpecialCharacter.IsEnabled)
            {
                RefreshSpecialCharacterMarkers(instance, EMono.scene, EMono._zone, EMono.pc, characters, judgement, setting.SpecialCharacter);
            }

            // 特殊アイテム描画
            if (setting.SpecialThing.IsEnabled)
            {
                RefreshSpecialThingMarkers(instance, EMono.scene, EMono._zone, EMono.pc, things, judgement, setting.SpecialThing);
            }

            // カスタム描画
            if (setting.Custom.IsEnabled)
            {
                RefreshCustomMarkers(instance, EMono.scene, EMono._zone, EMono.pc, characters, things, judgement, setting.Custom);
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
                if (marker.ParticleSystem is not null)
                {
                    UpdateParticleSystem(instance, EMono.scene, EMono._zone, marker.ParticleSystem, marker.MarkerShape);
                }
            }
        }


        #endregion
    }
}

using Elin.Plugin.Generated;
using Elin.Plugin.Main.Models.Impl;
using Elin.Plugin.Main.Models.Settings;
using Elin.Plugin.Main.PluginHelpers;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Elin.Plugin.Main.Models
{
    public enum ColorElement
    {
        R,
        G,
        B,
        A,
    }

    public class MinimapMenu
    {
        public MinimapMenu(WidgetMinimap widgetMinimap, UIContextMenu parentMenu, Setting setting)
        {
            WidgetMinimap = widgetMinimap;
            ParentMenu = parentMenu;
            Setting = setting;
        }

        #region property

        private WidgetMinimap WidgetMinimap { get; }
        private UIContextMenu ParentMenu { get; }
        private Setting Setting { get; }

        private bool CalledApplySetting { get; set; } = false;

        // 色アイコンのキャッシュ（キーは Hex 文字列）
        private Dictionary<string, Sprite> ColorIconCache { get; } = new();

        IReadOnlyCollection<Color> Colors { get; } = new[]
        {
            // Row 1 (濃い系)
            new Color(0f, 0f, 0f, 1f),                 // Black
            new Color(0.502f, 0.502f, 0.502f, 1f),     // DarkGray (128,128,128)
            new Color(0.502f, 0f, 0f, 1f),             // Maroon (128,0,0)
            new Color(1f, 0f, 0f, 1f),                 // Red
            new Color(1f, 0.502f, 0f, 1f),             // Orange (255,128,0)
            new Color(1f, 1f, 0f, 1f),                 // Yellow
            new Color(0f, 0.502f, 0f, 1f),             // Green (0,128,0)
            new Color(0f, 0.753f, 0.753f, 1f),         // Teal/Cyan (0,192,192)
            new Color(0f, 0.314f, 1f, 1f),             // Blue (0,80,255)
            new Color(0.627f, 0.125f, 0.941f, 1f),     // Purple (160,32,240)

            // Row 2 (淡め / 肌合い)
            new Color(1f, 1f, 1f, 1f),                 // White
            new Color(0.753f, 0.753f, 0.753f, 1f),     // LightGray (192,192,192)
            new Color(0.588f, 0.294f, 0f, 1f),         // Brown (150,75,0)
            new Color(1f, 0.753f, 0.796f, 1f),         // Pink (255,192,203)
            new Color(1f, 0.976f, 0.769f, 1f),         // LightYellow / Cream (255,249,196)
            new Color(0.871f, 0.722f, 0.529f, 1f),     // Tan (222,184,135)
            new Color(0.667f, 1f, 0.667f, 1f),         // LightGreen (170,255,170)
            new Color(0.400f, 0.851f, 1f, 1f),         // LightCyan (102,217,255)
            new Color(0.502f, 0.784f, 1f, 1f),         // SkyBlue (128,200,255)
            new Color(0.902f, 0.784f, 1f, 1f),         // Lavender (230,200,255)
        };

        #endregion

        #region function

        private Sprite GetOrCreateColorSprite(string key, Color color)
        {
            if (ColorIconCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            TextureUtility.Fill(texture, color);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            ColorIconCache[key] = sprite;
            return sprite;
        }


        private void ApplySetting(bool rebuild)
        {
            if (rebuild)
            {
                // キャッシュ構築のための内部データビルド
                Setting.Custom.Build(true);
                WidgetMinimapImpl.Rebuild(WidgetMinimap, Setting);
            }

            WidgetMinimap.RefreshStyle();
            WidgetMinimap.Reload();

            CalledApplySetting = true;
        }

        private static Color ChangeColor(Color sourceColor, ColorElement colorElement, float value)
        {
            return new Color(
                colorElement == ColorElement.R ? value : sourceColor.r,
                colorElement == ColorElement.G ? value : sourceColor.g,
                colorElement == ColorElement.B ? value : sourceColor.b,
                colorElement == ColorElement.A ? value : sourceColor.a
            );
        }


        private void AddColorMenus(UIContextMenu menuItem, bool useAlpha, bool useColorList, Color color, Action<Color> onChange)
        {
            void AddMenu(ColorElement colorElement)
            {
                var colorValue = colorElement switch
                {
                    ColorElement.R => color.r,
                    ColorElement.G => color.g,
                    ColorElement.B => color.b,
                    ColorElement.A => color.a,
                    _ => throw new ArgumentOutOfRangeException(nameof(colorElement), colorElement, null)
                };
                menuItem.AddSlider(colorElement.ToString(), a => a.ToString(), colorValue, a =>
                {
                    var newColor = ChangeColor(color, colorElement, a);
                    onChange(newColor);
                }, min: 0, max: 1);
            }

            AddMenu(ColorElement.R);
            AddMenu(ColorElement.G);
            AddMenu(ColorElement.B);
            if (useAlpha)
            {
                AddMenu(ColorElement.A);
            }
            if (useColorList)
            {
                var colorListMenu = menuItem.AddChild(ModHelper.Lang.General.SettingCommonColorList);

                foreach (var c in Colors)
                {
                    var hex = UnityUtility.ToHexColor(c);
                    var menu = colorListMenu.AddButton(string.Empty, () =>
                    {
                        onChange(c);
                    });

                    // なんか知らんがメニューいっぱいに伸びてるのでもうこれでいいや
                    var sprite = GetOrCreateColorSprite(hex, c);
                    var gameObject = new GameObject("icon");
                    var image = gameObject.AddComponent<Image>();
                    image.sprite = sprite;
                    gameObject.transform.SetParent(menu.transform, false);
                    menu.icon = image;
                }
            }
        }

        private void ApplyStairsMenu(UIContextMenu parentMenu, StairsMarkerSetting setting)
        {
            var currentMenu = parentMenu.AddChild(ModHelper.Lang.General.SettingStairs);

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonIsEnabled, setting.IsEnabled, a =>
            {
                setting.IsEnabled = a;
                ApplySetting(false);
            });

            var upMenu = currentMenu.AddChild(ModHelper.Lang.General.SettingStairsUp);
            AddColorMenus(upMenu, true, true, setting.UpColor, a =>
            {
                setting.UpColor = a;
                ApplySetting(false);
            });

            var downMenu = currentMenu.AddChild(ModHelper.Lang.General.SettingStairsDown);
            AddColorMenus(downMenu, true, true, setting.DownColor, a =>
            {
                setting.DownColor = a;
                ApplySetting(false);
            });

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonShowEvenIfUnseen, setting.ShowEvenIfUnseen, a =>
            {
                setting.ShowEvenIfUnseen = a;
                ApplySetting(false);
            });
        }

        private void ApplyMinionMenu(UIContextMenu parentMenu, MarkerSetting setting)
        {
            var currentMenu = parentMenu.AddChild(ModHelper.Lang.General.SettingMinion);

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonIsEnabled, setting.IsEnabled, a =>
            {
                setting.IsEnabled = a;
                ApplySetting(false);
            });

            AddColorMenus(currentMenu, true, true, setting.Color, a =>
            {
                setting.Color = a;
                ApplySetting(false);
            });
        }

        private void ApplyPetMenu(UIContextMenu parentMenu, MarkerSetting setting)
        {
            var currentMenu = parentMenu.AddChild(ModHelper.Lang.General.SettingPet);

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonIsEnabled, setting.IsEnabled, a =>
            {
                setting.IsEnabled = a;
                ApplySetting(false);
            });

            AddColorMenus(currentMenu, true, true, setting.Color, a =>
            {
                setting.Color = a;
                ApplySetting(false);
            });
        }

        private void ApplySpecialCharacterMenu(UIContextMenu parentMenu, SpecialCharacterMarkerSetting setting)
        {
            var currentMenu = parentMenu.AddChild(ModHelper.Lang.General.SettingSpecialCharacter);

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonIsEnabled, setting.IsEnabled, a =>
            {
                setting.IsEnabled = a;
                ApplySetting(false);
            });

            var bossMenu = currentMenu.AddChild(ModHelper.Lang.General.SettingSpecialCharacterBoss);
            AddColorMenus(bossMenu, true, true, setting.BossColor, a =>
            {
                setting.BossColor = a;
                ApplySetting(false);
            });

            var evolvedMenu = currentMenu.AddChild(ModHelper.Lang.General.SettingSpecialCharacterEvolved);
            AddColorMenus(evolvedMenu, true, true, setting.EvolvedColor, a =>
            {
                setting.EvolvedColor = a;
                ApplySetting(false);
            });

            var bigDaddyRow = EClass.sources.charas.map[ElinId.BigDaddyId];
            var bigDaddyMenu = currentMenu.AddChild(bigDaddyRow.GetName());
            AddColorMenus(bigDaddyMenu, true, true, setting.BigDaddyColor, a =>
            {
                setting.BigDaddyColor = a;
                ApplySetting(false);
            });

            var santaRow = EClass.sources.charas.map[ElinId.SantaId];
            var santaMenu = currentMenu.AddChild(santaRow.GetName());
            AddColorMenus(santaMenu, true, true, setting.SantaColor, a =>
            {
                setting.SantaColor = a;
                ApplySetting(false);
            });

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonShowEvenIfUnseen, setting.ShowEvenIfUnseen, a =>
            {
                setting.ShowEvenIfUnseen = a;
                ApplySetting(false);
            });
        }

        private void ApplySpecialThingMenu(UIContextMenu parentMenu, SpecialThingMarkerSetting setting)
        {
            var currentMenu = parentMenu.AddChild(ModHelper.Lang.General.SettingSpecialThing);

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonIsEnabled, setting.IsEnabled, a =>
            {
                setting.IsEnabled = a;
                ApplySetting(false);
            });
            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonIgnoreSelfZone, setting.IgnoreSelfZone, a =>
            {
                setting.IgnoreSelfZone = a;
                ApplySetting(false);
            });

            var statuePowerRow = EClass.sources.cards.map[ElinId.StatuePower];
            var shrineMenu = currentMenu.AddChild(statuePowerRow.GetName());
            AddColorMenus(shrineMenu, true, true, setting.ShrineColor, a =>
            {
                setting.ShrineColor = a;
                ApplySetting(false);
            });

            var statueGodRow = EClass.sources.cards.map[ElinId.StatueGod];
            var godMenu = currentMenu.AddChild(statueGodRow.GetName());
            AddColorMenus(godMenu, true, true, setting.GodColor, a =>
            {
                setting.GodColor = a;
                ApplySetting(false);
            });

            var waterJureRow = EClass.sources.cards.map[ElinId.WaterJure];
            var jureMenu = currentMenu.AddChild(waterJureRow.GetName());
            AddColorMenus(jureMenu, true, true, setting.JureColor, a =>
            {
                setting.JureColor = a;
                ApplySetting(false);
            });

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonShowEvenIfUnseen, setting.ShowEvenIfUnseen, a =>
            {
                setting.ShowEvenIfUnseen = a;
                ApplySetting(false);
            });
        }

        private void ApplyCustomMenu(UIContextMenu parentMenu, CustomMarkerSetting setting)
        {
            void InputDialog(string current, Action<string> inputed)
            {
                Dialog.InputName(string.Empty, current, (cancel, text) =>
                {
                    if (!cancel)
                    {
                        inputed(text.Trim());
                        ApplySetting(true);
                    }
                }, Dialog.InputType.Default);
            }

            var currentMenu = parentMenu.AddChild(ModHelper.Lang.General.SettingCustom);

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonIsEnabled, setting.IsEnabled, a =>
            {
                setting.IsEnabled = a;
                ApplySetting(setting.IsEnabled);
            });
            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonIgnoreSelfZone, setting.IgnoreSelfZone, a =>
            {
                setting.IgnoreSelfZone = a;
                ApplySetting(!setting.IgnoreSelfZone);
            });

            var characterMenu = currentMenu.AddChild(ModHelper.Lang.General.SettingCustomCharacter);
            characterMenu.AddButton(ModHelper.Lang.Formatter.FormatSettingCustomEdit(value: setting.Character.Csv), () =>
            {
                InputDialog(setting.Character.Csv, a => setting.Character.Csv = a);
            });
            characterMenu.AddSeparator();
            AddColorMenus(characterMenu, true, true, setting.Character.Color, a =>
            {
                setting.Character.Color = a;
                ApplySetting(false);
            });

            var thingMenu = currentMenu.AddChild(ModHelper.Lang.General.SettingCustomThing);
            thingMenu.AddButton(ModHelper.Lang.Formatter.FormatSettingCustomEdit(value: setting.Thing.Csv), () =>
            {
                InputDialog(setting.Thing.Csv, a => setting.Thing.Csv = a);
            });
            thingMenu.AddSeparator();
            AddColorMenus(thingMenu, true, true, setting.Thing.Color, a =>
            {
                setting.Thing.Color = a;
                ApplySetting(false);
            });

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonShowEvenIfUnseen, setting.ShowEvenIfUnseen, a =>
            {
                setting.ShowEvenIfUnseen = a;
                ApplySetting(setting.ShowEvenIfUnseen);
            });
        }

        private void ApplyMapRefreshMenu(UIContextMenu parentMenu, MapRefreshSetting setting)
        {
            var currentMenu = parentMenu.AddChild(ModHelper.Lang.General.SettingMapRefresh);

            void AddIntervalMenu(string name, double value, Action<double> onChange)
            {
                currentMenu.AddSlider(name, a => ModHelper.Lang.Formatter.FormatSettingMapRefreshIntervalDisplay(seconds: a), (float)value, a =>
                {
                    onChange(a);
                    ApplySetting(false);
                }, (float)SettingDefine.MinimumPrimitiveInterval, (float)SettingDefine.MaximumPrimitiveInterval);
            }

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonIsEnabled, setting.IsEnabled, a =>
            {
                setting.IsEnabled = a;
                ApplySetting(false);
            });

            AddIntervalMenu(ModHelper.Lang.Formatter.FormatSettingMapRefreshInterval(breakpoint: ModHelper.Lang.General.SettingCommonBreakpointSmall), setting.Breakpoints.Small.PrimitiveInterval, a =>
            {
                setting.Breakpoints.Small.PrimitiveInterval = a;
            });
            AddIntervalMenu(ModHelper.Lang.Formatter.FormatSettingMapRefreshInterval(breakpoint: ModHelper.Lang.General.SettingCommonBreakpointMedium), setting.Breakpoints.Medium.PrimitiveInterval, a =>
            {
                setting.Breakpoints.Medium.PrimitiveInterval = a;
            });
            AddIntervalMenu(ModHelper.Lang.Formatter.FormatSettingMapRefreshInterval(breakpoint: ModHelper.Lang.General.SettingCommonBreakpointLarge), setting.Breakpoints.Large.PrimitiveInterval, a =>
            {
                setting.Breakpoints.Large.PrimitiveInterval = a;
            });
            AddIntervalMenu(ModHelper.Lang.Formatter.FormatSettingMapRefreshInterval(breakpoint: ModHelper.Lang.General.SettingCommonBreakpointFallback), setting.PrimitiveFallbackInterval, a =>
            {
                setting.PrimitiveFallbackInterval = a;
            });
        }

        private void ApplyPointerOverMenu(UIContextMenu parentMenu, PointerOverSetting setting)
        {
            var currentMenu = parentMenu.AddChild(ModHelper.Lang.General.SettingPointerOver);

            currentMenu.AddToggle(ModHelper.Lang.General.SettingCommonIsEnabled, setting.IsEnabled, a =>
            {
                setting.IsEnabled = a;
                ApplySetting(false);
            });

            void AddAroundMenu(string name, int value, Action<int> onChange)
            {
                currentMenu.AddSlider(name, a => a.ToString(), value, a =>
                {
                    onChange((int)a);
                    ApplySetting(false);
                }, SettingDefine.MinimumAroundCount, SettingDefine.MaximumAroundCount, isInt: true);
            }

            AddAroundMenu(ModHelper.Lang.Formatter.FormatSettingPointerOverAroundCount(breakpoint: ModHelper.Lang.General.SettingCommonBreakpointSmall), setting.Breakpoints.Small.AroundCount, a =>
            {
                setting.Breakpoints.Small.AroundCount = a;
            });
            AddAroundMenu(ModHelper.Lang.Formatter.FormatSettingPointerOverAroundCount(breakpoint: ModHelper.Lang.General.SettingCommonBreakpointMedium), setting.Breakpoints.Medium.AroundCount, a =>
            {
                setting.Breakpoints.Medium.AroundCount = a;
            });
            AddAroundMenu(ModHelper.Lang.Formatter.FormatSettingPointerOverAroundCount(breakpoint: ModHelper.Lang.General.SettingCommonBreakpointLarge), setting.Breakpoints.Large.AroundCount, a =>
            {
                setting.Breakpoints.Large.AroundCount = a;
            });
            AddAroundMenu(ModHelper.Lang.Formatter.FormatSettingPointerOverAroundCount(breakpoint: ModHelper.Lang.General.SettingCommonBreakpointFallback), setting.FallbackAroundCount, a =>
            {
                setting.FallbackAroundCount = a;
            });

            currentMenu.AddSeparator();

            currentMenu.AddSlider(ModHelper.Lang.General.SettingPointerOverMaxShownPerGroup, a => a.ToString(), setting.MaxShownPerGroup, a =>
            {
                setting.MaxShownPerGroup = (int)a;
                ApplySetting(false);
            }, SettingDefine.MinimumMaxShownPerGroup, SettingDefine.MaximumMaxShownPerGroup, isInt: true);

        }

        private void ApplyOthersMenu(UIContextMenu parentMenu, Setting setting)
        {
            var currentMenu = parentMenu.AddChild(ModHelper.Lang.General.SettingOthers);

            currentMenu.AddToggle(ModHelper.Lang.General.SettingOthersIsForegroundPlayer, setting.IsForegroundPlayer, a =>
            {
                setting.IsForegroundPlayer = a;
                ApplySetting(false);
            });

            currentMenu.AddToggle(ModHelper.Lang.General.SettingOthersAllowMoveWhenUnseen, setting.AllowMoveWhenUnseen, a =>
            {
                setting.AllowMoveWhenUnseen = a;
                ApplySetting(false);
            });

            var resetMenu = currentMenu.AddChild(ModHelper.Lang.General.SettingOthersReset);
            resetMenu.AddButton(ModHelper.Lang.General.SettingOthersResetExecute, () =>
            {
                // まぁなんというか、メニューが閉じてインスタンスから Setting が参照されなくなることに依存しまくってる処理
                // 中で呼ばれてるのは Setting.Instance なので致命傷でセーフ
                Setting.Reset();
                ApplySetting(true);
            });
        }

        public void Apply()
        {
            var rootMenu = ParentMenu.AddChild(Package.Title);

            ApplyStairsMenu(rootMenu, Setting.Stairs);
            ApplyMinionMenu(rootMenu, Setting.Minion);
            ApplyPetMenu(rootMenu, Setting.Pet);
            ApplySpecialCharacterMenu(rootMenu, Setting.SpecialCharacter);
            ApplySpecialThingMenu(rootMenu, Setting.SpecialThing);
            ApplyCustomMenu(rootMenu, Setting.Custom);
            ApplyMapRefreshMenu(rootMenu, Setting.MapRefresh);
            ApplyPointerOverMenu(rootMenu, Setting.PointerOver);
            ApplyOthersMenu(rootMenu, Setting);
        }

        #endregion
    }
}

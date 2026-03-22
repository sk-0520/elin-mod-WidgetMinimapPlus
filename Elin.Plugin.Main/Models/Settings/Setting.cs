using Elin.Plugin.Generated;
using Newtonsoft.Json;
using System;

namespace Elin.Plugin.Main.Models.Settings
{
    /// <summary>
    /// 設定。
    /// </summary>
    [GeneratePluginConfig]
    public partial class Setting
    {
        #region property

        [JsonIgnore]
        internal static Setting Instance { get; set; } = new Setting();

        /// <summary>
        /// 階段マーカー。
        /// </summary>
        public StairsMarkerSetting Stairs { get; set; } = new StairsMarkerSetting
        {
            IsEnabled = true,
            ShowEvenIfUnseen = false,
            UpColor = new Color(1, 1, 1, 0.6f),
            DownColor = new Color(1, 1, 1),
        };

        /// <summary>
        /// ミニオンマーカー。
        /// </summary>
        public MarkerSetting Minion { get; set; } = new MarkerSetting
        {
            IsEnabled = true,
            Color = new Color(0.0f, 1.0f, 1.0f, 0.4f),
        };

        /// <summary>
        /// ペットマーカー。
        /// </summary>
        public MarkerSetting Pet { get; set; } = new MarkerSetting
        {
            IsEnabled = true,
            Color = new Color(0.0f, 1.0f, 1.0f),
        };

        /// <summary>
        /// 特別キャラクター。
        /// </summary>
        public SpecialCharacterMarkerSetting SpecialCharacter { get; set; } = new SpecialCharacterMarkerSetting
        {
            IsEnabled = true,
            ShowEvenIfUnseen = false,
            BigDaddyColor = new Color(1.0f, 0.0f, 0.0f),
            SantaColor = new Color(1.0f, 0.0f, 0.0f),
            BossColor = new Color(1.0f, 0.7f, 0.0f),
            EvolvedColor = new Color(0.65f, 0.0f, 1.0f),
        };

        /// <summary>
        /// 特別アイテム。
        /// </summary>
        public SpecialThingMarkerSetting SpecialThing { get; set; } = new SpecialThingMarkerSetting()
        {
            IsEnabled = true,
            ShowEvenIfUnseen = false,
            IgnoreSelfZone = true,
            ShrineColor = new Color(1.0f, 0.85f, 0.45f),
            GodColor = new Color(1.0f, 1.0f, 0.0f),
            JureColor = new Color(0, 1, 0),
        };

        public CustomMarkerSetting Custom { get; set; } = new CustomMarkerSetting
        {
            IsEnabled = true,
            ShowEvenIfUnseen = false,
            IgnoreSelfZone = true,
            Character = new CustomMarkerItemSetting
            {
                Csv = string.Empty,
                Color = new Color(0.5f, 0.5f, 1),
            },
            Thing = new CustomMarkerItemSetting
            {
                Csv = string.Empty,
                Color = new Color(0.9f, 1, 0.6f),
            },
        };

        public MapRefreshSetting MapRefresh { get; set; } = new MapRefreshSetting
        {
            IsEnabled = true,
            Breakpoints = new MapRefreshBreakpointsSetting()
            {
                Small = new MapRefreshBreakpointSetting
                {
                    Width = 20,
                    Interval = TimeSpan.FromSeconds(1),
                },
                Medium = new MapRefreshBreakpointSetting
                {
                    Width = 45,
                    Interval = TimeSpan.FromSeconds(1.5),
                },
                Large = new MapRefreshBreakpointSetting
                {
                    Width = 90,
                    Interval = TimeSpan.FromSeconds(2),
                },
            },
            FallbackInterval = TimeSpan.FromSeconds(3),
        };

        public PointerOverSetting PointerOver { get; set; } = new PointerOverSetting
        {
            IsEnabled = true,
            Breakpoints = new PointerOverBreakpointsSetting
            {
                Small = new PointerOverBreakpointSetting
                {
                    Width = 20,
                    AroundCount = 2,
                },
                Medium = new PointerOverBreakpointSetting
                {
                    Width = 45,
                    AroundCount = 4,
                },
                Large = new PointerOverBreakpointSetting
                {
                    Width = 90,
                    AroundCount = 6,
                },
            },
            FallbackAroundCount = 8,
            MaxShownPerGroup = 4,
        };

        /// <summary>
        /// PCを最上位描画とするか。
        /// </summary>
        /// <remarks>
        /// 再描画してるだけ！
        /// </remarks>
        public virtual bool IsForegroundPlayer { get; set; } = true;

        /// <summary>
        /// まだ見ていなくてもキャラクターを移動可能にするか。
        /// </summary>
        public virtual bool AllowMoveWhenUnseen { get; set; } = false;

        #endregion

        #region function

        /// <summary>
        /// 設定データのうち、毎回処理するには重いものを事前処理。
        /// </summary>
        /// <param name="outputMessage"></param>
        internal void Build(bool outputMessage)
        {
            Custom.Build(outputMessage);
        }

        #endregion
    }
}

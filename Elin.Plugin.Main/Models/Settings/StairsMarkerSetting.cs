using Elin.Plugin.Main.Converters;
using Newtonsoft.Json;

namespace Elin.Plugin.Main.Models.Settings
{
    /// <summary>
    /// 階段用マーカー設定。
    /// </summary>
    public class StairsMarkerSetting
    {
        #region property

        /// <summary>
        /// 有効。
        /// </summary>
        public virtual bool IsEnabled { get; set; }

        /// <summary>
        /// まだ見ていなくても表示するか。
        /// </summary>
        public virtual bool ShowEvenIfUnseen { get; set; }

        /// <summary>
        /// 上り階段色。
        /// </summary>
        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color UpColor { get; set; }
        /// <summary>
        /// 下り階段色。
        /// </summary>
        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color DownColor { get; set; }

        #endregion
    }

}

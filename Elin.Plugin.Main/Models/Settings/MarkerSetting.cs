using Elin.Plugin.Main.Converters;
using Newtonsoft.Json;

namespace Elin.Plugin.Main.Models.Settings
{
    /// <summary>
    /// 汎用マーカー設定。
    /// </summary>
    public class MarkerSetting
    {
        #region property

        /// <summary>
        /// 有効。
        /// </summary>
        public virtual bool IsEnabled { get; set; }

        /// <summary>
        /// 色。
        /// </summary>
        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color Color { get; set; }

        #endregion
    }

}

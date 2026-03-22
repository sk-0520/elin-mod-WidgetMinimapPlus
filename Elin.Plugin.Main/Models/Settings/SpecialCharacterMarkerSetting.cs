using Elin.Plugin.Main.Converters;
using Newtonsoft.Json;

namespace Elin.Plugin.Main.Models.Settings
{
    public class SpecialCharacterMarkerSetting
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
        /// ボスの色。
        /// </summary>
        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color BossColor { get; set; }

        /// <summary>
        /// 進化した敵の色。
        /// </summary>
        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color EvolvedColor { get; set; }

        /// <summary>
        /// ビッグダディの色。
        /// </summary>
        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color BigDaddyColor { get; set; }

        /// <summary>
        /// サンタの色。
        /// </summary>
        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color SantaColor { get; set; }

        #endregion
    }

}

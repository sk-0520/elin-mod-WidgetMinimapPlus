using Elin.Plugin.Main.Converters;
using Newtonsoft.Json;

namespace Elin.Plugin.Main.Models.Settings
{
    public class SpecialThingMarkerSetting
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
        /// 自拠点では無視するか。
        /// </summary>
        /// <remarks>
        /// <para>自分のテント判定ってないような気がする。</para>
        /// <para>TODO: いつか探してみる。</para>
        /// </remarks>
        public virtual bool IgnoreSelfZone { get; set; }

        /// <summary>
        /// 祠の色。
        /// </summary>
        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color ShrineColor { get; set; }

        /// <summary>
        /// 神像の色。
        /// </summary>
        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color GodColor { get; set; }

        /// <summary>
        /// ジュア水の色。
        /// </summary>
        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color JureColor { get; set; }

        #endregion
    }
}

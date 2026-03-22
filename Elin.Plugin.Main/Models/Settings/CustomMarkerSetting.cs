using BepInEx.Logging;
using Elin.Plugin.Generated;
using Elin.Plugin.Main.Converters;
using Elin.Plugin.Main.PluginHelpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Elin.Plugin.Main.Models.Settings
{
    public class CustomMarkerArtifact
    {
        #region property

        public List<Regex> Regexes { get; } = new List<Regex>();

        #endregion
    }

    public class CustomMarkerItemSetting
    {
        #region property

        public virtual string Csv { get; set; } = string.Empty;

        [JsonConverter(typeof(UnityColorConverter))]
        public virtual Color Color { get; set; }

        [JsonIgnore]
        [IgnorePluginConfig]
        internal CustomMarkerArtifact Artifact { get; set; } = new CustomMarkerArtifact();

        #endregion

        #region function

        internal void Build(string configItemName, bool outputMessage)
        {
            Artifact.Regexes.Clear();

            if (string.IsNullOrWhiteSpace(Csv))
            {
                return;
            }

            var errorValues = new List<string>();
            var valueParser = new CsvRegexParser();
            var csvValues = valueParser.ParseCsv(Csv);
            foreach (var csvValue in csvValues)
            {
                if (valueParser.TryGetRegex(csvValue, out var regex))
                {
                    Artifact.Regexes.Add(regex!);
                }
                else
                {
                    errorValues.Add(csvValue);
                }
            }

            if (0 < errorValues.Count)
            {
                ModHelper.LogNotify(
                    LogLevel.Warning,
                    ModHelper.Lang.Formatter.FormatCustomSettingError(
                        configItemGroup: ModHelper.Lang.General.SettingCustom,
                        configItemName: configItemName,
                        errorValues: string.Join(", ", errorValues)
                    ),
                    outputMessage
                );
            }

        }

        #endregion
    }

    /// <summary>
    /// カスタムマーカー。
    /// </summary>
    public class CustomMarkerSetting
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

        public virtual bool PrioritizeThing { get; set; }

        public CustomMarkerItemSetting Character { get; set; } = new CustomMarkerItemSetting();
        public CustomMarkerItemSetting Thing { get; set; } = new CustomMarkerItemSetting();

        #endregion

        internal void Build(bool outputMessage)
        {
            Character.Build(ModHelper.Lang.General.SettingCustomCharacter, outputMessage);
            Thing.Build(ModHelper.Lang.General.SettingCustomThing, outputMessage);
        }
    }
}

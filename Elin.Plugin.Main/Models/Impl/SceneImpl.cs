using Elin.Plugin.Main.Models.Settings;
using Elin.Plugin.Main.PluginHelpers;
using Newtonsoft.Json;

namespace Elin.Plugin.Main.Models.Impl
{
    public static class SceneImpl
    {
        #region Scene

        public static void InitPostfix(Scene instance, Scene.Mode newMode, Setting setting)
        {
            if (newMode == Scene.Mode.StartGame)
            {
                // プラグイン読み込み時にはまだ設定のビルドを行っていないため、プロファイル開始時のこのタイミングで実施
                setting.Build(true);

                ModHelper.DoDev(() =>
                {
                    ModHelper.WriteDev(ModHelper.GetInformationString());
                    var json = JsonConvert.SerializeObject(setting, Formatting.Indented);
                    ModHelper.WriteDev(json);
                });
            }
        }

        #endregion
    }
}

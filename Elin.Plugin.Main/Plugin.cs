using Elin.Plugin.Main.Models.Settings;
using Elin.Plugin.Main.PluginHelpers;

// Mod 用テンプレート組み込み想定

namespace Elin.Plugin.Main
{
    partial class Plugin
    {
        #region function

        private Setting LoadConfig()
        {
            var setting = Setting.Bind(Config, new Setting());
            return setting;
        }

        /// <summary>
        /// 起動時のプラグイン独自処理。
        /// </summary>
        private void AwakePlugin()
        {
            var setting = LoadConfig();
            Setting.Instance = setting;
            // 設定のビルド自体はプロファイル読み込み時に行う
        }

        /// <summary>
        /// 終了時のプラグイン独自処理。
        /// </summary>
        private void OnDestroyPlugin()
        {
            //NOP
        }

#if DEBUG
        public void PHL()
        {
            ModHelper.LogDev("PHL!");
            Setting.Instance.Build(MessageHelper.CanOutputMessage);
        }
#endif

        #endregion
    }
}

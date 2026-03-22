using BepInEx;
using Elin.Plugin.Generated;
using Elin.Plugin.Main.Models.Settings;
using Elin.Plugin.Main.PluginHelpers;
using HarmonyLib;
using Newtonsoft.Json;

// Mod 用テンプレート組み込み想定

namespace Elin.Plugin.Main
{
    [BepInPlugin(Package.Id, Mod.Name, Mod.Version)]
    public class Plugin : BaseUnityPlugin
    {
        #region proeprty

        private bool UseConfig { get; } = false;

        #endregion

        #region function

        private Setting LoadConfig()
        {
            var setting = Setting.Bind(Config, new Setting());
            return setting;
        }

        /// <summary>
        /// 起動時のプラグイン独自処理。
        /// </summary>
        /// <param name="harmony"></param>
        private void AwakePlugin(Harmony harmony)
        {
            var setting = LoadConfig();
            Setting.Instance = setting;

            ModHelper.DoDebug(() =>
            {
                var json = JsonConvert.SerializeObject(setting, Formatting.Indented);
                ModHelper.WriteDebug(json);
            });
        }

        /// <summary>
        /// 終了時のプラグイン独自処理。
        /// </summary>
        private void OnDestroyPlugin()
        {
            //NOP
        }

        /// <summary>
        /// 起動。
        /// </summary>
        /// <remarks>本メソッドではインフラ面の構築も行っているため、プラグインとしての起動処理は <see cref="AwakePlugin(Harmony)"/> で実施する。</remarks>
        public void Awake()
        {
            ModHelper.Initialize(this, Logger);

            var harmony = new Harmony(Package.Id);

            AwakePlugin(harmony);

            harmony.PatchAll();
        }

        public void OnDestroy()
        {
            OnDestroyPlugin();
            ModHelper.Destroy();
        }

        #endregion
    }

#if DEBUG

    // 以下はパッチ適用のサンプル。

    [HarmonyPatch(typeof(Scene))]
    public class ScenePatch
    {
        #region function

        [HarmonyPatch(nameof(Scene.Init), new[] { typeof(Scene.Mode) })]
        [HarmonyPostfix]
        public static void InitPostfix(Scene.Mode newMode)
        {
            if (newMode == Scene.Mode.StartGame)
            {
                ModHelper.LogDebug("DEBUG START");
                ModHelper.LogDebug(ModHelper.ToStringFromInformation());
            }
        }

        #endregion
    }

#endif

}

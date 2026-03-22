using Elin.Plugin.Main.Models.Impl;
using Elin.Plugin.Main.Models.Settings;
using HarmonyLib;

namespace Elin.Plugin.Main.Patches
{
    [HarmonyPatch(typeof(Scene))]
    public static class ScenePatch
    {
        #region Scene

        [HarmonyPatch(nameof(Scene.Init), new[] { typeof(Scene.Mode) })]
        [HarmonyPostfix]
        public static void InitPostfix(Scene __instance, Scene.Mode newMode)
        {
            SceneImpl.InitPostfix(__instance, newMode, Setting.Instance);
        }

        #endregion
    }
}

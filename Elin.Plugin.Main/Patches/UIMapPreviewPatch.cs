using Elin.Plugin.Main.Models.Impl;
using Elin.Plugin.Main.Models.Settings;
using HarmonyLib;

namespace Elin.Plugin.Main.Patches
{
    [HarmonyPatch(typeof(UIMapPreview))]
    public static class UIMapPreviewPatch
    {
        [HarmonyPatch(nameof(UIMapPreview._RefreshPoint), new[] { typeof(int), typeof(int), typeof(bool) })]
        [HarmonyPrefix]
        public static void _RefreshPointPrefix(UIMapPreview __instance, ref Color ___colorStairs, int x, int z, bool apply)
        {
            UIMapPreviewImpl._RefreshPointPrefix(__instance, ref ___colorStairs, Setting.Instance, x, z, apply);
        }

        [HarmonyPatch(nameof(UIMapPreview._RefreshPoint), new[] { typeof(int), typeof(int), typeof(bool) })]
        [HarmonyPostfix]
        public static void _RefreshPointPostfix(UIMapPreview __instance, ref Color ___colorStairs, int x, int z, bool apply)
        {
            UIMapPreviewImpl._RefreshPointPostfix(__instance, ref ___colorStairs, Setting.Instance, x, z, apply);
        }
    }
}

using Elin.Plugin.Main.Models.Impl;
using Elin.Plugin.Main.Models.Settings;
using HarmonyLib;
using UnityEngine.EventSystems;

namespace Elin.Plugin.Main.Patches
{

    [HarmonyPatch(typeof(WidgetMinimap))]
    public static class WidgetMinimapPatch
    {
        #region function

        [HarmonyPatch(nameof(WidgetMinimap.OnActivate))]
        [HarmonyPrefix]
        public static void OnActivatePrefix(WidgetMinimap __instance)
        {
            WidgetMinimapImpl.OnActivatePrefix(__instance);
        }

        [HarmonyPatch(nameof(WidgetMinimap.OnActivate))]
        [HarmonyPostfix]
        public static void OnActivatePostfix(WidgetMinimap __instance)
        {
            WidgetMinimapImpl.OnActivatePostfix(__instance, Setting.Instance);
        }

        [HarmonyPatch(nameof(WidgetMinimap.OnMoveZone))]
        [HarmonyPostfix]
        public static void OnMoveZonePostfix(WidgetMinimap __instance)
        {
            WidgetMinimapImpl.OnMoveZonePostfix(__instance, Setting.Instance);
        }

#if false
        private static Stopwatch _stopwatch = new Stopwatch();
        [HarmonyPatch(nameof(WidgetMinimap.Reload))]
        [HarmonyPrefix]
        public static void ReloadPrefix()
        {
            _stopwatch.Start();
        }

        [HarmonyPatch(nameof(WidgetMinimap.Reload))]
        [HarmonyPostfix]
        public static void ReloadPostfix()
        {
            ModHelper.LogDebug($"ちっくたっく: {_stopwatch.Elapsed}");
        }
#endif
        [HarmonyPatch(nameof(WidgetMinimap.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostfix(WidgetMinimap __instance)
        {
            WidgetMinimapImpl.OnUpdatePostfix(__instance, Setting.Instance);
        }

        [HarmonyPatch(nameof(WidgetMinimap.OnPointerDown), new[] { typeof(PointerEventData) })]
        [HarmonyPrefix]
        public static bool OnPointerDownPrefix(WidgetMinimap __instance, PointerEventData e)
        {
            return WidgetMinimapImpl.OnPointerDownPrefix(__instance, e, Setting.Instance);
        }

        [HarmonyPatch(nameof(WidgetMinimap.OnSetContextMenu), new[] { typeof(UIContextMenu) })]
        [HarmonyPrefix]
        public static void OnSetContextMenuPostfix(WidgetMinimap __instance, UIContextMenu m)
        {
            WidgetMinimapImpl.OnSetContextMenuPostfix(__instance, m, Setting.Instance);
        }

        [HarmonyPatch(nameof(WidgetMinimap.RefreshMarkers))]
        [HarmonyPostfix]
        public static void RefreshMarkersPostfix(WidgetMinimap __instance)
        {
            WidgetMinimapImpl.RefreshMarkersPostfix(__instance, Setting.Instance);
        }

        [HarmonyPatch(nameof(WidgetMinimap.RefreshStyle))]
        [HarmonyPostfix]
        public static void RefreshStylePostfix(WidgetMinimap __instance)
        {
            WidgetMinimapImpl.RefreshStylePostfix(__instance, Setting.Instance);
        }

        #endregion
    }
}

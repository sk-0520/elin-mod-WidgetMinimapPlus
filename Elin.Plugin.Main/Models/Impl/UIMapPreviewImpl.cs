using Elin.Plugin.Main.Models.Settings;
using Elin.Plugin.Main.PluginHelpers;
using System.Linq;

namespace Elin.Plugin.Main.Models.Impl
{
    public static class UIMapPreviewImpl
    {
        #region property

        /// <summary>
        /// Elin 側で作成された階段の色。
        /// </summary>
        /// <remarks><see cref="_RefreshPointPrefix"/>実行時にフィールドの書き換えを行う都合上、設定変更の適用時に元に戻せなくなるため、処理ごとに色の戻しを行うための保存用フィールド。</remarks>
        private static Color? ElinColorStairs { get; set; }

        #endregion

        #region UIMapPreview

        public static void _RefreshPointPrefix(UIMapPreview instance, ref Color colorStairs, Setting setting, int x, int z, bool apply)
        {
            if (!setting.Stairs.IsEnabled)
            {
                return;
            }

            if (ModHelper.Elin.IsGlobalMap(EMono.scene, EMono._zone))
            {
                return;
            }

            // マーカー側表示とずれるので階段ピクセルは通常の色にしておく
            var cell = instance.map.cells[x, z];

            var isStair = cell.Things.Any(a => a.Thing.trait is TraitStairs);
            if (isStair)
            {
                // [ELIN:UIMapPreviewImpl._RefreshPoint]
                // -> SourceMaterial.Row row = ((cell.bridgeHeight != 0) ? cell.matBridge : cell.matFloor)
                var row = (cell.bridgeHeight != 0) ? cell.matBridge : cell.matFloor;
                var color = row.GetColor();

                ElinColorStairs = colorStairs;

                colorStairs = color;
            }
        }

        public static void _RefreshPointPostfix(UIMapPreview instance, ref Color colorStairs, Setting setting, int x, int z, bool apply)
        {
            if (ElinColorStairs.HasValue)
            {
                colorStairs = ElinColorStairs.Value;
                ElinColorStairs = null;
            }
        }


        #endregion
    }
}

using Elin.Plugin.Generated;
using System;

namespace Elin.Plugin.Main.Models.Settings
{
    /// <summary>
    /// マップ更新処理設定。
    /// </summary>
    /// NOTE: ブレークポイント系の集合として class Breakpoints&lt;T&gt; small/medium/large を作っては見たものの、ジェネレーターがえぐいことになったので諦め
    /// でも記述の統一性とか考えると、なんらか強制できる方法はほしい
    public class PointerOverBreakpointSetting : IComparable<PointerOverBreakpointSetting>
    {
        #region property

        /// <summary>
        /// ゾーンサイズがこの値以下の場合に対象とする。
        /// </summary>
        [RangePluginConfig(SettingDefine.MinimumWidth, SettingDefine.MaximumWidth)]
        public virtual int Width { get; set; }

        /// <summary>
        /// 周辺数。
        /// </summary>
        [RangePluginConfig(SettingDefine.MinimumAroundCount, SettingDefine.MaximumAroundCount)]
        public virtual int AroundCount { get; set; }

        #endregion

        #region IComparable

        public int CompareTo(PointerOverBreakpointSetting? other)
        {
            if (other is null)
            {
                return 1;
            }

            return Width.CompareTo(other.Width);
        }

        #endregion
    }

    public class PointerOverBreakpointsSetting : BreakpointsBase<PointerOverBreakpointSetting>
    {
        //NOP
    }

    public class PointerOverSetting
    {
        #region property

        /// <summary>
        /// 有効。
        /// </summary>

        public virtual bool IsEnabled { get; set; }

        public PointerOverBreakpointsSetting Breakpoints { get; set; } = new PointerOverBreakpointsSetting();

        /// <summary>
        /// <see cref="Breakpoints"/> に該当しない場合の周辺数。
        /// </summary>
        [RangePluginConfig(SettingDefine.MinimumAroundCount, SettingDefine.MaximumAroundCount)]
        public virtual int FallbackAroundCount { get; set; }

        /// <summary>
        /// ツールチップに表示する各項目(ペット,キャラクター,アイテム)ごとの最大数。
        /// </summary>
        [RangePluginConfig(SettingDefine.MinimumMaxShownPerGroup, SettingDefine.MaximumMaxShownPerGroup)]
        public virtual int MaxShownPerGroup { get; set; }

        #endregion
    }
}

using Elin.Plugin.Generated;
using System;

namespace Elin.Plugin.Main.Models.Settings
{
    /// <summary>
    /// マップ更新処理設定。
    /// </summary>
    public class MapRefreshBreakpointSetting : IComparable<MapRefreshBreakpointSetting>
    {
        #region property

        /// <summary>
        /// ゾーンサイズがこの値以下の場合に対象とする。
        /// </summary>
        [RangePluginConfig(SettingDefine.MinimumWidth, SettingDefine.MaximumWidth)]
        public virtual int Width { get; set; }

        /// <summary>
        /// リフレッシュ間隔。
        /// </summary>
        [RangePluginConfig(SettingDefine.MinimumPrimitiveInterval, SettingDefine.MaximumPrimitiveInterval)]
        public virtual double PrimitiveInterval { get; set; }

        /// <inheritdoc cref="PrimitiveInterval"/>
        [IgnorePluginConfig]
        public TimeSpan Interval
        {
            get => TimeSpan.FromSeconds(PrimitiveInterval);
            set => PrimitiveInterval = value.TotalSeconds;
        }

        #endregion

        #region IComparable

        public int CompareTo(MapRefreshBreakpointSetting? other)
        {
            if (other is null)
            {
                return 1;
            }

            return Width.CompareTo(other.Width);
        }

        #endregion
    }

    public class MapRefreshBreakpointsSetting : BreakpointsBase<MapRefreshBreakpointSetting>
    {
        //NOP
    }

    public class MapRefreshSetting
    {
        #region property

        /// <summary>
        /// 有効。
        /// </summary>
        public virtual bool IsEnabled { get; set; }

        // MapRefreshBreakpointSetting[] is not supported by the config system. Supported types: String, Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal, Enum, Color, Vector2, Vector3, Vector4, Quaternion
        public MapRefreshBreakpointsSetting Breakpoints { get; set; } = new MapRefreshBreakpointsSetting();

        /// <summary>
        /// <see cref="Breakpoints"/> に該当しない場合のリフレッシュ間隔。
        /// </summary>
        [RangePluginConfig(SettingDefine.MinimumPrimitiveInterval, SettingDefine.MaximumPrimitiveInterval)]
        public virtual double PrimitiveFallbackInterval { get; set; }

        /// <inheritdoc cref="PrimitiveFallbackInterval"/>
        [IgnorePluginConfig]
        public TimeSpan FallbackInterval
        {
            get => TimeSpan.FromSeconds(PrimitiveFallbackInterval);
            set => PrimitiveFallbackInterval = value.TotalSeconds;
        }

        #endregion
    }
}

namespace Elin.Plugin.Main.Models
{
    /// <summary>
    /// X/Y座標がZ出てきて混乱するので一応。
    /// </summary>
    public readonly record struct Point2D
    {
        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        #region property

        public int X { get; }
        public int Y { get; }

        #endregion
    }
}

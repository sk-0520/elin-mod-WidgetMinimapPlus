using System;

namespace Elin.Plugin.Main.Models.Settings
{
    /// <summary>
    /// ブレークポイント統一用基底クラス。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// <para>ソースジェネレーターで単純にジェネリクスを使ったら頭吹っ飛びそうになったので野暮ったいけど一段かまして使用すること。</para>
    /// <para>元々ふつーに配列を使用していたが、BepInEx が配列ダメだと分かって、一つずつ手作りしてたんだけど、心が死んだのでこの形に落ち着いた。</para>
    /// </remarks>
    public abstract class BreakpointsBase<T>
        where T : class, IComparable<T>, new()
    {
        #region property

        public T Small { get; set; } = new T();
        public T Medium { get; set; } = new T();
        public T Large { get; set; } = new T();

        #endregion

        #region function

        public T[] GetSortedItems()
        {
            var items = new[] { Small, Medium, Large };
            Array.Sort(items);
            return items;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Elin.Plugin.Main.Models
{
    public struct MinimapTooltipItem<T>
    {
        public MinimapTooltipItem(IReadOnlyCollection<T> displayed, IReadOnlyCollection<T> hidden)
        {
            Displayed = displayed;
            Hidden = hidden;
        }

        #region property

        /// <summary>
        /// 表示対象項目一覧。
        /// </summary>
        public IReadOnlyCollection<T> Displayed { get; }
        /// <summary>
        /// 非表示項目対象一覧。
        /// </summary>
        public IReadOnlyCollection<T> Hidden { get; }

        #endregion
    }

    public class MinimapTooltip
    {
        public MinimapTooltip(IReadOnlyCollection<Chara> pets, IReadOnlyCollection<Chara> specialCharacters, IReadOnlyCollection<Thing> specialThings, IReadOnlyCollection<Chara> customCharacters, IReadOnlyCollection<Thing> customItems)
        {
            Pets = pets;
            SpecialCharacters = specialCharacters;
            SpecialThings = specialThings;
            CustomCharacters = customCharacters;
            CustomThings = customItems;

            IsShowable = 0 < Pets.Count
                || 0 < SpecialCharacters.Count
                || 0 < SpecialThings.Count
                || 0 < CustomCharacters.Count
                || 0 < CustomThings.Count
            ;
        }

        #region proeprty

        /// <summary>
        /// 表示すべき項目があるか。
        /// </summary>
        public bool IsShowable { get; }

        public IReadOnlyCollection<Chara> Pets { get; }
        public IReadOnlyCollection<Chara> SpecialCharacters { get; }
        public IReadOnlyCollection<Thing> SpecialThings { get; }
        public IReadOnlyCollection<Chara> CustomCharacters { get; }
        public IReadOnlyCollection<Thing> CustomThings { get; }

        #endregion

        #region function

        private MinimapTooltipItem<T> GetItems<T>(IReadOnlyCollection<T> source, int maxShown, Point2D center)
            where T : Card
        {
            if (maxShown <= 0)
            {
                // 表示数が0以下ならすべて Hidden
                return new MinimapTooltipItem<T>(Array.Empty<T>(), source);
            }

            // 元の順序を保持するためにインデックスを付与して安定ソートを実現
            var indexed = source
                .Select((item, index) =>
                {
                    var dx = item.Cell.x - center.X;
                    var dy = item.Cell.z - center.Y;

                    // 距離は二乗で比較すれば sqrt を回避できる
                    var distSq = dx * dx + dy * dy;

                    return (item, distSq, index);
                });

            var ordered = indexed
                .OrderBy(x => x.distSq)
                .ThenBy(x => x.index)
                .ToArray();

            var displayed = ordered.Take(maxShown).Select(x => x.item).ToArray();
            var hidden = ordered.Skip(maxShown).Select(x => x.item).ToArray();

            return new MinimapTooltipItem<T>(displayed, hidden);
        }

        public MinimapTooltipItem<Chara> GetPets(int maxShown, Point2D center)
        {
            return GetItems(Pets, maxShown, center);
        }

        public MinimapTooltipItem<Chara> GetSpecialCharacters(int maxShown, Point2D center)
        {
            return GetItems(SpecialCharacters, maxShown, center);
        }

        public MinimapTooltipItem<Thing> GetSpecialThings(int maxShown, Point2D center)
        {
            return GetItems(SpecialThings, maxShown, center);
        }

        public MinimapTooltipItem<Chara> GetCustomCharacters(int maxShown, Point2D center)
        {
            return GetItems(CustomCharacters, maxShown, center);
        }

        public MinimapTooltipItem<Thing> GetCustomItems(int maxShown, Point2D center)
        {
            return GetItems(CustomThings, maxShown, center);
        }

        #endregion
    }
}

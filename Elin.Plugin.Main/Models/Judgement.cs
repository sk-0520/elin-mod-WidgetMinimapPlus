using Elin.Plugin.Main.Models.Settings;
using System.Linq;

namespace Elin.Plugin.Main.Models
{
    /// <summary>
    /// キャラクター・アイテムの判定処理。
    /// </summary>
    public class Judgement
    {
        #region function

        /// <summary>
        /// 該当位置は視認済みか。
        /// </summary>
        /// <param name="cell">セル。</param>
        /// <param name="ignoreUnseen">視認済みとする。</param>
        /// <returns></returns>
        public bool IsSeen(Cell cell, bool ignoreUnseen)
        {
            if (ignoreUnseen)
            {
                return true;
            }

            return cell.isSeen;
        }

        /// <summary>
        /// 検索対象のキャラクターか。
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool IsSearchableCharacter(Chara character)
        {
            return !character.IsPC;
        }

        /// <summary>
        /// 検索対象のアイテムか。
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public bool IsSearchableThing(Thing thing)
        {
            return !thing.isRoofItem;
        }


        /// <summary>
        /// 階段か。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsStair(Thing target)
        {
            // ロックされた階段はいいんじゃないかな

            return target.trait is TraitStairs;
        }

        /// <summary>
        /// 所有者のミニオンか。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="ownerCharacter">ミニオンの所有者。</param>
        /// <returns></returns>
        public bool IsMinion(Chara target, Chara ownerCharacter)
        {
            // [ELIN:Zone.CountMinions]
            // -> if (chara.c_uidMaster == c.uid && chara.c_minionType == MinionType.Default)
            // なんで if 分割したんだろう
            if (target.c_uidMaster != ownerCharacter.uid)
            {
                return false;
            }
            if (target.c_minionType != MinionType.Default)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ペットか。
        /// </summary>
        /// <remarks>寄生・騎乗状態はペットとは扱わない。</remarks>
        /// <param name="target"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        public bool IsPet(Chara target, Chara playerCharacter)
        {
            // PCパーティでなければ無視
            if (!target.IsPCParty)
            {
                return false;
            }

            // ペットに騎乗している場合は無視
            if (playerCharacter.ride == target)
            {
                return false;
            }
            // ペットを寄生している場合は無視
            if (playerCharacter.parasite == target)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 特別キャラクターか。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsSpecialCharacter(Chara target)
        {
            if (target.IsPCParty)
            {
                return false;
            }

            // ボス
            if (target.c_bossType == BossType.Boss)
            {
                return true;
            }

            // 進化した敵
            if (target.c_bossType == BossType.Evolved)
            {
                return true;
            }

            // 定義済みキャラクター
            if (ElinId.SpecialCharacterIds.Contains(target.id))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 特別アイテムか。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsSpecialThing(Thing target)
        {
            if (!ElinId.SpecialThingIds.Contains(target.id))
            {
                return false;
            }

            // 宝箱判定
            if (ElinId.SpecialThingTreasureChestIds.Contains(target.id))
            {
                // 中身の有無で要不要判定
                return 0 < target.things.Count;
            }

            // 使用済みのものは不要
            if (!target.trait.owner.isOn)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 独自設定したキャラクター候補か。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        public bool IsCustomCharacterCandidate(Chara target, Chara playerCharacter)
        {
            if (IsMinion(target, playerCharacter))
            {
                return false;
            }

            if (IsPet(target, playerCharacter))
            {
                return false;
            }

            if (IsSpecialCharacter(target))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 独自設定したアイテム候補か。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsCustomThingCandidate(Thing target)
        {
            if (IsStair(target))
            {
                return false;
            }

            if (IsSpecialThing(target))
            {
                return false;
            }

            return true;
        }

        // あとで設定追加するかもしれないので第二引数は大きくしといた
        public bool IsCustomElement<T>(T element, CustomMarkerItemSetting setting)
            where T : Card
        {
            foreach (var regex in setting.Artifact.Regexes)
            {
                if (regex.IsMatch(element.Name) || regex.IsMatch(element.id))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}

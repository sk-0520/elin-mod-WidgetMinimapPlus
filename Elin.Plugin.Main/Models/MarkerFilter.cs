using Elin.Plugin.Main.Models.Settings;
using Elin.Plugin.Main.PluginHelpers;

namespace Elin.Plugin.Main.Models
{
    /// <summary>
    /// マーカーフィルター。
    /// </summary>
    /// <remarks>フレーム毎に生成する想定。</remarks>
    public class MarkerFilter
    {
        public MarkerFilter(Judgement judgement, Zone zone)
        {
            Judgement = judgement;
            Zone = zone;

            NowSelfZone = ModHelper.Elin.IsSelfZone(Zone);
            NowDefenseGame = ModHelper.Elin.IsDefenseGame(Zone);
        }

        #region property

        public Judgement Judgement { get; }
        private Zone Zone { get; }

        /// <summary>
        /// 現在は自拠点か。
        /// </summary>
        private bool NowSelfZone { get; }
        /// <summary>
        /// 現在は戦争中か。
        /// </summary>
        private bool NowDefenseGame { get; }


        #endregion

        #region function

        /// <summary>
        /// 自拠点を無視するか。
        /// </summary>
        /// <param name="ignoreSelfZone">設定情報。</param>
        /// <returns></returns>
        private bool IsSelfZone(bool ignoreSelfZone)
        {
            return ignoreSelfZone && NowSelfZone;
        }

        private bool IsStair(Thing thing, bool showEvenIfUnseen)
        {
            if (!Judgement.IsStair(thing))
            {
                return false;
            }

            if (!Judgement.IsSeen(thing.Cell, showEvenIfUnseen))
            {
                return false;
            }

            return thing.trait is TraitStairs;
        }


        public bool IsUpStair(Thing thing, StairsMarkerSetting setting)
        {
            if (!IsStair(thing, setting.ShowEvenIfUnseen))
            {
                return false;
            }

            return thing.trait is TraitStairsUp;
        }

        public bool IsDownStair(Thing thing, StairsMarkerSetting setting)
        {
            if (!IsStair(thing, setting.ShowEvenIfUnseen))
            {
                return false;
            }

            return !(thing.trait is TraitStairsUp);
        }

        public bool IsMinion(Chara character, Chara playerCharacter, MarkerSetting setting)
        {
            return Judgement.IsMinion(character, playerCharacter);
        }

        public bool IsPet(Chara character, Chara playerCharacter, MarkerSetting setting)
        {
            return Judgement.IsPet(character, playerCharacter);
        }

        public bool SpecialCharacter(Chara character, SpecialCharacterMarkerSetting setting)
        {
            // 戦争中は既存のマーカーと衝突するので無視
            if (NowDefenseGame)
            {
                return false;
            }

            if (!Judgement.IsSpecialCharacter(character))
            {
                return false;
            }

            if (!Judgement.IsSeen(character.Cell, setting.ShowEvenIfUnseen))
            {
                return false;
            }

            return true;
        }

        public bool IsSpecialThing(Thing thing, SpecialThingMarkerSetting setting)
        {
            if (IsSelfZone(setting.IgnoreSelfZone))
            {
                return false;
            }

            if (!Judgement.IsSpecialThing(thing))
            {
                return false;
            }

            if (!Judgement.IsSeen(thing.Cell, setting.ShowEvenIfUnseen))
            {
                return false;
            }

            return true;
        }

        public bool IsCustomCharacterCandidate(Chara character, Chara playerCharacter, CustomMarkerSetting setting)
        {
            if (IsSelfZone(setting.IgnoreSelfZone))
            {
                return false;
            }

            if (!Judgement.IsCustomCharacterCandidate(character, playerCharacter))
            {
                return false;
            }

            if (!Judgement.IsSeen(character.Cell, setting.ShowEvenIfUnseen))
            {
                return false;
            }

            return true;
        }

        public bool IsCustomThingCandidate(Thing thing, CustomMarkerSetting setting)
        {
            if (IsSelfZone(setting.IgnoreSelfZone))
            {
                return false;
            }

            if (!Judgement.IsCustomThingCandidate(thing))
            {
                return false;
            }

            if (!Judgement.IsSeen(thing.Cell, setting.ShowEvenIfUnseen))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}

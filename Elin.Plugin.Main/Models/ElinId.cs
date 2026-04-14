using System.Collections.Generic;

namespace Elin.Plugin.Main.Models
{
    /// <summary>
    /// Elin 定義 ID。
    /// </summary>
    /// <remarks>Mod 内で必要なものだけ定義。</remarks>
    public static class ElinId
    {
        static ElinId()
        {
            // SpecialThingIds の構築
            var specialThingIds = new List<string>(SpecialThingTreasureChestIds.Count + SpecialThingGodOnlyIds.Count + SpecialThingOtherIds.Count);
            specialThingIds.AddRange(SpecialThingTreasureChestIds);
            specialThingIds.AddRange(SpecialThingGodOnlyIds);
            specialThingIds.AddRange(SpecialThingOtherIds);
            SpecialThingIds = specialThingIds;
        }

        #region property

        /// <summary>
        /// [特殊キャラクター] ビッグダディ。
        /// </summary>
        /// <remarks>SourceChara.xlsx!Chara</remarks>
        public const string BigDaddyId = "big_daddy";
        /// <summary>
        /// [特殊キャラクター] サンタクロース。
        /// </summary>
        /// <remarks>SourceChara.xlsx!Chara</remarks>
        public const string SantaId = "santa";

        /// <summary>
        /// 特殊キャラクター一覧。
        /// </summary>
        public static readonly string[] SpecialCharacterIds = [BigDaddyId, SantaId];

        /// <summary>
        /// [特殊アイテム] 宝箱。
        /// </summary>
        /// <remarks>SourceCard.xlsx!Thing</remarks>
        public const string TreasureChestNormal = "chest3";
        /// <summary>
        /// [特殊アイテム] 真珠貝。
        /// </summary>
        /// <remarks>SourceCard.xlsx!Thing</remarks>
        public const string TreasureChestPearl = "pearl_oyster";
        /// <summary>
        /// [特殊アイテム] 豪華な宝箱。
        /// </summary>
        /// <remarks>SourceCard.xlsx!Thing</remarks>
        public const string TreasureChestBoss = "chest_boss";
        /// <summary>
        /// [特殊アイテム] 神秘的な宝箱。
        /// </summary>
        /// <remarks>SourceCard.xlsx!Thing</remarks>
        public const string TreasureChestGorgeous = "chest_treasure";


        /// <summary>
        /// [特殊アイテム] 癒しの女神像。
        /// </summary>
        /// <remarks>SourceCard.xlsx!ThingV</remarks>
        public const string StatueGodJure = "659";
        /// <summary>
        /// [特殊アイテム] 元素の神像。
        /// </summary>
        /// <remarks>SourceCard.xlsx!ThingV</remarks>
        public const string StatueGodItzpalt = "758";
        /// <summary>
        /// [特殊アイテム] 大地の神像。
        /// </summary>
        /// <remarks>SourceCard.xlsx!ThingV</remarks>
        public const string StatueGodOpatos = "759";
        /// <summary>
        /// [特殊アイテム] 収穫の神像。
        /// </summary>
        /// <remarks>SourceCard.xlsx!ThingV</remarks>
        public const string StatueGodKumiromi = "806";
        /// <summary>
        /// [特殊アイテム] 幸運の女神像。
        /// </summary>
        /// <remarks>SourceCard.xlsx!ThingV</remarks>
        public const string StatueGodEhekatl = "828";
        /// <summary>
        /// [特殊アイテム] 風の女神像。
        /// </summary>
        /// <remarks>SourceCard.xlsx!ThingV</remarks>
        public const string StatueGodLulwy = "1190";
        /// <summary>
        /// [特殊アイテム] 機械の神像。
        /// </summary>
        /// <remarks>SourceCard.xlsx!ThingV</remarks>
        public const string StatueGodMani = "1191";
        /// <summary>
        /// [特殊アイテム] 祠。
        /// </summary>
        /// <remarks>SourceCard.xlsx!Thing</remarks>
        public const string StatuePower = "statue_power";
        /// <summary>
        /// [特殊アイテム] 神像。
        /// </summary>
        /// <remarks>
        /// <para>SourceCard.xlsx!Thing</para>
        /// <para>アイテム判定自体は ThingV の方でこれは名前を取得するために使用。</para>
        /// </remarks>
        public const string StatueGod = "statue_god";
        /// <summary>
        /// [特殊アイテム] ジュア水。
        /// </summary>
        /// <remarks>
        /// <para>SourceCard.xlsx!Thing</para>
        /// <para>見たことないから知らん。</para>
        /// </remarks>
        public const string WaterJure = "water_jure";

        /// <summary>
        /// [特殊アイテム] 宝箱一覧。
        /// </summary>
        public static readonly IReadOnlyList<string> SpecialThingTreasureChestIds = [
            TreasureChestNormal,
            TreasureChestPearl,
            TreasureChestBoss,
            TreasureChestGorgeous,
        ];

        /// <summary>
        /// [特殊アイテム] 神像一覧。
        /// </summary>
        public static readonly IReadOnlyList<string> SpecialThingGodOnlyIds = [
            StatueGodJure,
            StatueGodItzpalt,
            StatueGodOpatos,
            StatueGodKumiromi,
            StatueGodEhekatl,
            StatueGodLulwy,
            StatueGodMani,
        ];

        /// <summary>
        /// [特殊アイテム] その他一覧。
        /// </summary>
        public static readonly IReadOnlyList<string> SpecialThingOtherIds = [
            StatuePower,
            WaterJure,
        ];

        /// <summary>
        /// 特殊キャラクター一覧。
        /// </summary>
        // <remarks>静的コンストラクタで初期化</remarks>
        public static readonly IReadOnlyList<string> SpecialThingIds;

        #endregion
    }
}

using Elin.Plugin.Generated;

namespace Elin.Plugin.Main.Test.Models.Setting
{
    // 設定というよりジェネレータ確認用

    public partial class Nested
    {
        [RangePluginConfig(10, 20)]
        public virtual int I1 { get; set; }
        [ListPluginConfig(2, 4, 6, 8)]
        public virtual int I2 { get; set; }

        [RangePluginConfig(0.0f, 1.5f)]
        public virtual float F1 { get; set; }
        [ListPluginConfig(1.0f, 100.0f, 1000.0f)]
        public virtual float F2 { get; set; }
        [RangePluginConfig(-1, -2)]
        public virtual float F3 { get; set; }
        [ListPluginConfig(10, 100, 1000)]
        public virtual float F4 { get; set; }

        [RangePluginConfig(0.0, 1.5)]
        public virtual double D1 { get; set; }
        [RangePluginConfig(0.0f, 1.5f)]
        public virtual double D2 { get; set; }
        [ListPluginConfig(10.5, 100.5, 1000.5)]
        public virtual double D3 { get; set; }

        /// <summary>
        /// あれやこれや。<see cref="D3"/> 参照。
        /// <para>あうあう <see cref="D2"/> です。・</para>
        /// </summary>
        [ListPluginConfig("a", "b", "c")]
        public virtual string S { get; set; } = string.Empty;
    }

    [GeneratePluginConfig]
    public partial class Test
    {
        public Nested NestedA { get; set; } = new Nested();
        public Nested NestedB { get; set; } = new Nested();
    }

    //public class SettingTest
    //{
    //}
    //internal class Gen
    //{
    //    public void Test1(string a, string b)
    //    {
    //        //NOP
    //    }
    //    [RequireNamedArguments]
    //    public void Test2(string a, string b)
    //    {
    //        //NOP
    //    }

    //    public void A()
    //    {
    //        Test1("a", "b");
    //        Test2("a", "b");
    //        Test2(a: "a", "b");
    //        Test2("a", b: "b");
    //    }
    //}
}

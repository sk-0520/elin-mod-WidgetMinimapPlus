using Elin.Plugin.Generated;

namespace Elin.Plugin.Generator.Test.Use.PluginInfoGeneratorUseTestSandbox
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852", Justification = "子は partial 強制はないもののプロキシ作成が必要なので sealed 指定はダメ")]
    internal class ChildConfig
    {
        #region property

        public virtual int Data { get; set; }

        #endregion
    }

    [GeneratePluginConfig]
    internal partial class NestConfig
    {
        #region property

        public ChildConfig ChildA { get; set; } = new ChildConfig();
        public ChildConfig ChildB { get; set; } = new ChildConfig();

        public virtual int Value { get; set; }

        #endregion
    }
}

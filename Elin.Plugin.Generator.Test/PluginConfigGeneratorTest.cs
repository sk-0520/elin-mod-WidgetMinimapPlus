using Elin.Plugin.Generator.Test.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Elin.Plugin.Generator.Test
{
    public class PluginConfigGeneratorTest
    {
        #region function

        [Fact]
        public void BasicTest()
        {
            var generator = new PluginConfigGenerator();
            var driver = CSharpGeneratorDriver.Create(
                generator.AsSourceGenerator()
            );

            var inputCompilation = TestCompilation.Create<PluginConfigGenerator>([
                CSharpSyntaxTree.ParseText(
                    //lang=c#
                    """
                    using Elin.Plugin.Generated;

                    [GeneratePluginConfig]
                    public partial class Config
                    {
                        public virtual string Number { get; set; }
                    }
                    """,
                    path: "Config.cs",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            ]);

            var generatorDriver = driver.RunGeneratorsAndUpdateCompilation(
                inputCompilation,
                out var outputCompilation,
                out var diagnostics,
                TestContext.Current.CancellationToken
            );
            var runResult = generatorDriver.GetRunResult();
            var generatorResult = runResult.Results.FirstOrDefault();

            Assert.True(diagnostics.IsEmpty);
            Assert.Null(generatorResult.Exception);

            // 3ファイルが生成される
            Assert.Equal(3, generatorResult.GeneratedSources.Length);
            // これはソースジェネレータ側で作成するが動的に何か変わることはないので生成された事実だけを受け止めて、ノータッチ
            Assert.Contains(generatorResult.GeneratedSources, a => a.HintName == "Attributes.g.cs");

            // 実際に Config から生成されたやつらをテストしていく
            var actualConfig = generatorResult.GeneratedSources.First(a => a.HintName == "Config.g.cs");
            var actualConfigBind = generatorResult.GeneratedSources.First(a => a.HintName == "Config.bind.g.cs");

            var configRoot = actualConfig.SyntaxTree.GetRoot(TestContext.Current.CancellationToken);

            // ユーザー使用型とビルド時使用型の確認
            var configClassDeclarations = configRoot
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .ToArray()
            ;
            Assert.Contains(configClassDeclarations, a => a.Identifier.Text == "ConfigConfigEntries");
            Assert.Contains(configClassDeclarations, a => a.Identifier.Text == "ConfigProxy");

            // はい OK
            // 生成自体はOKなので、使用側テストで挙動確認する
        }

        #endregion
    }
}

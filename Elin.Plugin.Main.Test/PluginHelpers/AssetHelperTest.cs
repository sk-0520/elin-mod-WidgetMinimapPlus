using Elin.Plugin.Main.PluginHelpers;
using System.IO;

namespace Elin.Plugin.Main.Test.PluginHelpers
{
    public class AssetHelperTest
    {
        #region function

        [Fact]
        public void CombineArg1Test()
        {
            var test = new AssetHelper("dir");

            var actual = test.Combine("path1");

            var expected = Path.Combine("dir", "path1");

            Assert.Equal(expected, actual);
        }

        public static TheoryData<string, string, string, string[]> CombineArg2Data => new()
        {
            {
                Path.Combine("dir", "path1", "path2"),
                "dir",
                "path1",
                new [] {
                    "path2",
                }
            },
            {
                Path.Combine("dir", "path1", "path2", "path3"),
                "dir",
                "path1",
                new [] {
                    "path2",
                    "path3",
                }
            },
            {
                Path.Combine("dir", "path1", "path2", "path3", "path4"),
                "dir",
                "path1",
                new [] {
                    "path2",
                    "path3",
                    "path4",
                }
            },
        };

        [Theory]
        [MemberData(nameof(CombineArg2Data))]
        public void CombineArg2Test(string expected, string dir, string path1, string[] path2)
        {
            var test = new AssetHelper(dir);

            var actual = test.Combine(path1, path2);

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}

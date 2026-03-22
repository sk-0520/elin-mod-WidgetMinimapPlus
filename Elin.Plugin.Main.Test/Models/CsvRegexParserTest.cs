using Elin.Plugin.Main.Models;
using System.Text.RegularExpressions;

namespace Elin.Plugin.Main.Test.Models
{
    public class CsvRegexParserTest
    {
        #region function

        [Theory]
        [InlineData(new string[0], "")]
        [InlineData(new string[0], ",")]
        [InlineData(new[] { "a" }, "a")]
        [InlineData(new[] { "a", "b" }, "a,b")]
        [InlineData(new[] { "a", "c" }, "a,,c")]
        [InlineData(new string[0], " ")]
        [InlineData(new[] { "a" }, " a ")]
        [InlineData(new[] { "a" }, " a , ,")]
        [InlineData(new[] { "a", "c" }, "a, ,c")]
        public void ParseCsvTest(string[] expected, string input)
        {
            var test = new CsvRegexParser();
            var actual = test.ParseCsv(input);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a")]
        [InlineData("a*")]
        [InlineData("a/a")]
        [InlineData("a/a/")]
        public void TryGetRegex_Plain_Test(string input)
        {
            var test = new CsvRegexParser();
            var result = test.TryGetRegex(input, out var regex);
            Assert.True(result);
            Assert.NotNull(regex);
            var matches = regex.Matches(input);
            Assert.Single(matches);
        }

        [Theory]
        // 空文字はそもそも使用しない想定なのでテストしない
        [InlineData(false, RegexOptions.None, "/")]
        [InlineData(true, RegexOptions.IgnoreCase, "i")]
        [InlineData(true, RegexOptions.IgnoreCase, "ii")]
        [InlineData(true, RegexOptions.IgnorePatternWhitespace, "x")]
        [InlineData(true, RegexOptions.IgnorePatternWhitespace, "xx")]
        [InlineData(true, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, "ix")]
        [InlineData(true, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, "xi")]
        public void TryParseRegexOptionsTest(bool expectedSuccess, RegexOptions expectedOptions, string input)
        {
            var test = new CsvRegexParser();
            var actualResult = test.TryParseRegexOptions(input, out var actualOptions);
            Assert.Equal(expectedSuccess, actualResult);
            if (actualResult)
            {
                Assert.Equal(expectedOptions, actualOptions);
            }
        }

        [Theory]
        [InlineData(false, RegexOptions.None, "/")]
        [InlineData(false, RegexOptions.None, "//")]
        [InlineData(false, RegexOptions.None, "//flag")]
        [InlineData(true, RegexOptions.None, "/a/")]
        [InlineData(true, RegexOptions.IgnoreCase, "/a/i")]
        [InlineData(true, RegexOptions.IgnorePatternWhitespace, "/a/x")]
        [InlineData(true, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, "/a/ix")]
        public void TryGetRegex_Regex_Test(bool expectedSuccess, RegexOptions expectedOptions, string input)
        {
            var test = new CsvRegexParser();
            var actualResult = test.TryGetRegex(input, out var actualRegex);
            Assert.Equal(expectedSuccess, actualResult);
            if (actualResult)
            {
                Assert.NotNull(actualRegex);
                Assert.Equal(expectedOptions, actualRegex.Options);
            }
        }

        #endregion
    }
}

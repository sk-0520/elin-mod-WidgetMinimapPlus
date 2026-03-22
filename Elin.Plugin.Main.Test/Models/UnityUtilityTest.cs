using Elin.Plugin.Main.Models;

namespace Elin.Plugin.Main.Test.Models
{
    public class UnityUtilityTest
    {
        #region function

        public static TheoryData<string, Color> ToHexColorData => new(){
            {
                "#ff0000",
                new Color(1, 0, 0)
            },
            {
                "#00ff00",
                new Color(0, 1, 0)
            },
            {
                "#0000ff",
                new Color(0, 0, 1)
            },
            {
                "#000000",
                new Color(0, 0, 0)
            },
            {
                "#000000",
                new Color(0, 0, 0, 1)
            },
            {
                "#0000007f",
                new Color(0, 0, 0, 0.5f)
            },
        };
        [Theory]
        [MemberData(nameof(ToHexColorData))]
        public void ToHexColorTest(string expected, Color input)
        {
            var actual = UnityUtility.ToHexColor(input);
            Assert.Equal(expected, actual);
        }

        public static TheoryData<Color, string> ConvertFromHexColorData => new(){
            {
                new Color(1, 0, 0),
                "#ff0000"
            },
            {
                new Color(0, 1, 0),
                "#00ff00"
            },
            {
                new Color(0, 0, 1),
                "#0000ff"
            },
            {
                new Color(0, 0, 0),
                "#000000"
            },
            {
                new Color(0, 0, 0, 1),
                "#000000ff"
            },
            {
                new Color(0, 0, 0, 0.5f),
                "#00000080"
            },
        };
        [Theory]
        [MemberData(nameof(ConvertFromHexColorData))]
        public void ConvertFromHexColorTest(Color expected, string input)
        {
            var actual = UnityUtility.ConvertFromHexColor(input);

            float precision = 3; // まぁまぁ適当

            Assert.Equal(expected.r, actual.r, precision);
            Assert.Equal(expected.g, actual.g, precision);
            Assert.Equal(expected.b, actual.b, precision);
            Assert.Equal(expected.a, actual.a, precision);
        }

        #endregion
    }
}

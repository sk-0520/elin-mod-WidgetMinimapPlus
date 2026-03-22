namespace Elin.Plugin.Main.Models
{
    public static class UnityUtility
    {
        #region property

        public static string ToHexColor(Color color)
        {
            static string ToHex(float value)
            {
                return ((byte)(value * byte.MaxValue)).ToString("x2");
            }

            if (1 <= color.a)
            {
                return $"#{ToHex(color.r)}{ToHex(color.g)}{ToHex(color.b)}";
            }
            return $"#{ToHex(color.r)}{ToHex(color.g)}{ToHex(color.b)}{ToHex(color.a)}";
        }

        public static Color ConvertFromHexColor(string hexColor)
        {
            var hex = hexColor.TrimStart('#');
            float r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            float g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            float b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            float a = 8 <= hex.Length
                ? byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)
                : 0xff
            ;
            return new Color(r / byte.MaxValue, g / byte.MaxValue, b / byte.MaxValue, a / byte.MaxValue);
        }

        #endregion
    }
}

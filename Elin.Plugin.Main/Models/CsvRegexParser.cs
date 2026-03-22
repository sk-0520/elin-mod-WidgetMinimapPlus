using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Elin.Plugin.Main.Models
{
    public class CsvRegexParser
    {
        #region property

        private char[] Separator { get; } = new char[] { ',' };
        private Dictionary<char, RegexOptions> RegexOptionMap { get; } = new Dictionary<char, RegexOptions>()
        {
            ['i'] = RegexOptions.IgnoreCase,
            ['x'] = RegexOptions.IgnorePatternWhitespace,
        };
        private TimeSpan Timeout { get; } = TimeSpan.FromSeconds(1 / 60.0);

        #endregion

        #region function

        public IEnumerable<string> ParseCsv(string raw)
        {
            return raw.Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
            ;
        }

        private Regex ToPlainRegex(string plainText)
        {
            return new Regex(Regex.Escape(plainText), RegexOptions.None, Timeout);
        }

        public bool TryParseRegexOptions(string options, out RegexOptions result)
        {
            Debug.Assert(0 < options.Length);

            // private でいいけどテストしたかったのと、internal だとテスト側で競合で死ぬので public

            result = RegexOptions.None;

            foreach (var c in options)
            {
                if (RegexOptionMap.TryGetValue(c, out var option))
                {
                    result |= option;
                }
                else
                {
                    // 不明なオプションは無視せず失敗としておく
                    return false;
                }
            }

            return true;
        }

        public bool TryGetRegex(string pattern, out Regex? result)
        {
            Debug.Assert(0 < pattern.Length, $"{nameof(ParseCsv)}を通した値でから文字列はない");

            if (pattern[0] != '/')
            {
                result = ToPlainRegex(pattern);
                return true;
            }

            result = null;

            var lastIndex = pattern.LastIndexOf('/');
            if (lastIndex == -1)
            {
                // 正規表現扱いできない
                return false;
            }
            if (lastIndex <= 1)
            {
                // 末尾 / が先頭もしくは値を含まない
                return false;
            }

            // 正規表現扱い

            var regexPattern = pattern.Substring(1, lastIndex - 1);
            var regexOptions = RegexOptions.None;
            if (lastIndex + 1 < pattern.Length)
            {
                var rawOptions = pattern.Substring(lastIndex + 1);
                if (!TryParseRegexOptions(rawOptions, out regexOptions))
                {
                    return false;
                }
            }

            try
            {
                result = new Regex(regexPattern, regexOptions, Timeout);
                return true;
            }
            catch
            {
                // 失敗の種類はもう何でもいいので呼び出し側には失敗だけ伝える
                return false;
            }
        }

        #endregion
    }
}

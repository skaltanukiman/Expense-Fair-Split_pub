using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Commons
{
    public static class RegexPatterns
    {
        public static readonly string JapaneseText = @"^[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]+$";
        public static readonly string HiraganaAndKatakana = @"^[\p{IsHiragana}\p{IsKatakana}]+$";
        public static readonly string Hiragana = @"^[\p{IsHiragana}]+$";
        public static readonly string Katakana = @"^[\p{IsKatakana}]+$";
        public static readonly string JapaneseAndSingleByteAlphaNumeric = @"^[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}a-zA-Z0-9]+$";
        public static readonly string JapaneseAndDoubleByteAlphaNumeric = @"^[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}a-zA-Z0-9\uFF10-\uFF19\uFF21-\uFF3A\uFF41-\uFF5A]+$";
        public static readonly string RegexYenAmount = @"¥\s*(\d{1,3}(,\d{3})+|\d+)";
        public static readonly string RegexNumberWithComma = @"\d[\d,]*";
    }
}

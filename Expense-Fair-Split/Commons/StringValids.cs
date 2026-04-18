using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Commons
{
    internal class StringValids
    {
        /// <summary>
        /// 引数で渡された文字列がメールアドレスとしての要件を満たしているかをチェックします。
        /// </summary>
        /// <param name="email">チェック対象の文字列</param>
        /// <returns>
        ///     IsChecked = 条件を満たしているか否か
        ///     ErrType 1: 空文字かnull 2: メールアドレスとしての要件を満たしていない
        /// </returns>
        public static (bool, int) IsValidEmail(string email)
        {
            (bool IsChecked, int ErrType) result = (true, (int)EnumResource.IsValidEmailErrType.success);

            if (string.IsNullOrWhiteSpace(email)) 
            {
                result.ErrType = (int)EnumResource.IsValidEmailErrType.IsNullOrEmpty;
                result.IsChecked = false;
                return result;
            }

            // メールアドレスとしての様式を満たしているかをチェック
            try
            {
                var mailAddress = new MailAddress(email);
                return result;
            }
            catch (FormatException)
            {
                result.ErrType = (int)EnumResource.IsValidEmailErrType.FormatErr;
                result.IsChecked = false;
                return result;
            }
        }

        /// <summary>
        /// 引数で与えられたパターン値に応じた正規表現チェックを行い真偽値を返します。
        /// </summary>
        /// <param name="targetList">チェック対象の文字列</param>
        /// <param name="regexPattern">
        ///     0: 平仮名 + カタカナ + 漢字 + 半角英数字
        ///     1: 平仮名 + カタカナ + 漢字 + 半角全角英数字
        ///     2: 平仮名 + カタカナ + 漢字
        /// </param>
        /// <param name="blankFlg">
        ///     true: 空白を許可
        ///     false: 空白を不許可
        /// </param>
        /// <param name="errMsg"></param>
        /// <param name="errType">
        ///     0: 正常終了
        ///     1: リターンパターン不正値
        ///     2: 正規表現パターンなし
        ///     3: nullの検出
        ///     4: 正規表現パターンにマッチしない文字列の検出
        /// </param>
        /// <returns>パターンにマッチしたかを示す真偽値</returns>
        public static bool StrRegexPatternSelect(List<string> targetList, int regexPattern, bool blankFlg, out string errMsg, out int errType)
        {
            errMsg = string.Empty;
            errType = 0;
            string regex = string.Empty;
            bool result = true;

            switch (regexPattern)
            {
                case (int)EnumResource.RegexPatternSelect.JapaneseAndSingleByteAlphaNumeric:
                    regex = RegexPatterns.JapaneseAndSingleByteAlphaNumeric;
                    break;
                case (int)EnumResource.RegexPatternSelect.JapaneseAndDoubleByteAlphaNumeric:
                    regex = RegexPatterns.JapaneseAndDoubleByteAlphaNumeric;
                    break;
                case (int)EnumResource.RegexPatternSelect.JapaneseText:
                    regex = RegexPatterns.JapaneseText;
                    break;
                default:
                    errMsg = $"{nameof(StrRegexPatternSelect)}に指定された「正規表現パターン」が不適切です。";
                    errType = 1;
                    result = false;
                    return result;
            }

            if (string.IsNullOrWhiteSpace(regex))
            {
                errMsg = "「正規表現パターン」が入力されていません。";
                errType = 2;
                result = false;
                return result;
            }

            foreach (string target in targetList) 
            {
                if (target is null)
                {
                    errMsg = "nullが検出されました。";
                    errType = 3;
                    result = false;
                    return result;
                }

                if (blankFlg && target == string.Empty)
                {
                    continue;
                }

                if (!Regex.IsMatch(target, regex))
                {
                    errMsg += errMsg == string.Empty ? nameof(StrRegexPatternSelect) + "で検出:(" + target + "," : target + ",";
                    result = false;
                }
            }

            if (!result)
            {
                // チェック処理通過後、falseの場合はErrMsgの","を削除し末尾に")"を付与
                errMsg = errMsg.TrimEnd(',');
                errMsg = errMsg + ")";
                errType = 4;
            }

            return result;
        }

        /// <summary>
        /// 文字の先頭がホワイトスペース（空白や改行）かを判定します。
        /// </summary>
        /// <param name="targetVal">検証する文字列</param>
        /// <param name="isBrank">
        ///     検証する文字列がブランクやNullの場合、trueで返すかfalseで返すかを決める
        /// </param>
        /// <returns>
        ///     先頭の文字列がホワイトスペースの場合、true
        ///     違う場合、false
        /// </returns>
        public static bool IsFirstCharWhitespace(string? targetVal, bool isBrank)
        {
            // 何も入力がない場合は、引数によって返す真偽値を変える。
            if (string.IsNullOrEmpty(targetVal))
            {
                if (isBrank)
                {
                    return true;
                }
                else 
                {
                    return false;
                }
            }

            return char.IsWhiteSpace(targetVal[0]);
        }
    }
}

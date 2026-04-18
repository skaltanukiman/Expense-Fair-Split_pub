using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Services.Sessions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Commons
{
    public class CommonUtil
    {
        /// <summary>
        /// 指定された文字列リストをチェックし、
        /// 空またはnullの文字列が存在しないか調べます。
        /// </summary>
        /// <param name="strList">チェック対象の文字列リスト</param>
        /// <returns>
        /// 空またはnullが見つかった場合はエラーメッセージとfalseを、
        /// 見つからなかった場合、空の文字列とtrueを返します。
        /// </returns>
        public static (string, bool) StrNullOrEmptyCheck(List<string> strList)
        {
            (string ErrMsg, bool IsChecked) result = ("", false);
            
            if (strList is not null && strList.Count > 0)
            {
                foreach (string str in strList) 
                {
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        result.ErrMsg += $"{nameof(StrNullOrEmptyCheck)}でチェックされた値に空またはnullの文字列が検出されました。"; 
                        result.IsChecked = false;
                        return result;
                    }
                }
                result.IsChecked = true;
                return result;
            }
            else
            {
                result.ErrMsg += $"{nameof(StrNullOrEmptyCheck)}に渡されたListが不正です。";
                result.IsChecked = false;
                return result;
            }
        }

        /// <summary>
        /// 引数で渡された数値リストをチェックし、
        /// 指定モードの対象数値が存在するかを調べます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">チェック対象の数値リスト</param>
        /// <param name="mode">
        /// 0:リストの中身を0の数値が存在するかチェック
        /// 1:0または負の数が存在するかチェック
        /// </param>
        /// <returns>
        /// チェック対象の数値が存在した場合、エラーメッセージとfalseを、
        /// 存在しない場合は、空の文字列とtrueを返します。
        /// </returns>
        public static (string, bool) RejectZeroOrNegative<T>(List<T> items, int mode) where T : INumber<T>
        {
            (string ErrMsg, bool IsChecked) result = ("", false);
            if (items is not null && items.Count > 0)
            {
                foreach (T item in items)
                {
                    if (item is int || item is double || item is float || item is decimal)
                    {
                        switch (mode) 
                        {
                            case (int)EnumResource.ModeSelect.ZeroCheck:
                                if (item == T.Zero)
                                {
                                    result.ErrMsg += $"{nameof(RejectZeroOrNegative)}に0の数値が含まれています。";
                                    result.IsChecked = false;
                                    return result;
                                }
                                break;
                            case (int)EnumResource.ModeSelect.ZeroOrNegativeValue:
                                if (item == T.Zero || item < T.Zero)
                                {
                                    result.ErrMsg += $"{nameof(RejectZeroOrNegative)}に0または負の数が含まれています。";
                                    result.IsChecked = false;
                                    return result;
                                }
                                break;
                            default:
                                result.ErrMsg += "モードの指定が不適切です。";
                                result.IsChecked = false;
                                return result;
                        }

                    } else
                    {
                        result.ErrMsg += $"{nameof(RejectZeroOrNegative)}に渡された値に数値以外が含まれています。";
                        result.IsChecked = false;
                        return result;
                    }                    
                }
                result.IsChecked = true;
                return result;
            }
            else 
            {
                result.ErrMsg += $"{nameof(RejectZeroOrNegative)}に渡されたListが不正です。";
                result.IsChecked = false;
                return result;
            }
        }

        /// <summary>
        /// 引数で渡された文字列が空文字以外の場合、末尾に改行文字を挿入してリターンします。
        /// </summary>
        /// <param name="target">空文字かチェックをして、改行文字を挿入する文字列</param>
        /// <returns>改行文字挿入後のstring</returns>
        public static string InsertNewLineWhenNotBlank(string target)
        {
            target = target is null ? target = string.Empty : target;
            target = target == string.Empty ? target : target += "\n";
            return target;
        }

        /// <summary>
        /// 引数で渡された文字列リストをチェックし、指定文字数以内かを調べます。
        /// </summary>
        /// <param name="targetList">チェック対象の文字列リスト</param>
        /// <param name="maxChars">指定文字数</param>
        /// <param name="blankFlg">
        ///     空文字、nullを許容するかどうか
        ///     true: OK
        ///     false: NG
        /// </param>
        /// <returns>
        /// チェック対象の文字数が超過した場合、エラーメッセージとfalseを、
        /// 指定文字数以内の場合は、trueを返します。
        ///     ErrMsg = エラーメッセージ
        ///     IsChecked = チェック対象がチェック要件に見合っているか
        ///     ErrType = 0: 正常終了 1: 空文字、null終了 2: 文字数超過終了
        /// </returns>
        public static (string, bool, int) IsStringMaxLengthValid(List<string> targetList, int maxChars, bool blankFlg)
        {
            (string ErrMsg, bool IsChecked, int ErrType) result = (string.Empty, true, (int)EnumResource.IsStringLengthValidErrType.success);

            foreach (string target in targetList)
            {
                if (!blankFlg)
                {
                    if (string.IsNullOrWhiteSpace(target))
                    {
                        result.ErrMsg = $"ブランクが容認されていない({nameof(IsStringMaxLengthValid)})内で空文字、またはnull値が検出されました。";
                        result.IsChecked = false;
                        result.ErrType = (int)EnumResource.IsStringLengthValidErrType.IsNullOrEmpty;
                        
                        // blankNGの場合に、空文字、Nullが見つかった場合はその時点で処理を終了する。
                        return result;
                    }
                }

                if (string.IsNullOrWhiteSpace(target))
                {
                    continue;
                }

                if (target.Length > maxChars)
                {
                    result.ErrMsg += result.ErrMsg == string.Empty ? nameof(IsStringMaxLengthValid) + "で検出:(" + target + "," : target + ",";
                    result.IsChecked = false;
                }
            }
            
            if (!result.IsChecked)
            {
                // チェック処理通過後、falseの場合はErrMsgの","を削除し末尾に")"を付与
                result.ErrMsg = result.ErrMsg.TrimEnd(',');
                result.ErrMsg = result.ErrMsg + ")";
                result.ErrType = (int)EnumResource.IsStringLengthValidErrType.CharOverflow;
            }
            return result;
        }

        /// <summary>
        /// パスワード文字列をハッシュ化する
        /// </summary>
        /// <param name="password">ハッシュ化対象文字列</param>
        /// <param name="convSucceed">成功/失敗を表す真偽値</param>
        /// <returns>ハッシュ化されたString値</returns>
        public static string? HashPassword(string password, out bool convSucceed)
        {
            convSucceed = false;

            if (string.IsNullOrWhiteSpace(password)) 
            {
                return null;
            }

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            convSucceed = true;
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// この関数が呼び出された時点の東京（日本）の日付及び時刻を返します。
        /// </summary>
        /// <returns>DateTime型の日時</returns>
        public static DateTime CreateTokyoJapanCurrentDateTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
        }

        /// <summary>
        /// 引数で与えられたユーザーIDとセッションに保存されているユーザーIDを比較し、請求者、受領者を判別、真偽値として呼び出し元に返します。
        /// </summary>
        /// <param name="fromUserId">請求者のID</param>
        /// <param name="toUserId">受領者のID</param>
        /// <returns>
        ///     true:  請求者
        ///     false: 受領者
        ///     null:  どちらにも該当しない場合
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotUserSessionException"></exception>
        public static bool? CheckUserRoleInBilling(int fromUserId, int toUserId) 
        {
            var serviceProvider = App.Services;
            if (serviceProvider is null) throw new ArgumentNullException(nameof(serviceProvider));  // こちらはあとで変えるかも

            UserSessionService userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            if (userSessionService is null || userSessionService.UserId == -1) throw new NotUserSessionException();

            if (userSessionService.UserId == fromUserId) return true;
            if (userSessionService.UserId == toUserId) return false;
            return null;
        }

        /// <summary>
        /// 引数で与えられた真偽値とステータスコードに応じて適した文字列を返します。
        /// </summary>
        /// <param name="isPayer">
        ///     true:  請求者
        ///     false: 受領者
        ///     null:  該当なし
        /// </param>
        /// <param name="statusCode">現在の精算処理状態を表すコード</param>
        /// <returns>明細データの状態を表す文字列</returns>
        public static string GetBillingStatusMessage(bool? isPayer, int statusCode) 
        {
            if (!isPayer.HasValue)
            {
                return "取得できませんでした。";
            }

            switch (statusCode)
            {
                case -1:
                    return isPayer.Value ? "取消" : "取消";
                case 0:
                    return isPayer.Value ? "承認待ち" : "確認中";
                case 1:
                    return isPayer.Value ? "確認中" : "承認待ち";
                case 2:
                    return isPayer.Value ? "支払い待ち" : "未払い";
                case 3:
                    return isPayer.Value ? "承認待ち" : "確認中";
                case 100:
                    return isPayer.Value ? "完了" : "完了";
                default:
                    return "取得できませんでした。";
            }
        }

        /// <summary>
        /// 指定された区切り文字の左右にある文字を抽出します。
        /// </summary>
        /// <param name="targetStr">抽出対象</param>
        /// <param name="delimiter">指定区切り文字</param>
        /// <param name="leftBlank">抽出後ブランクを許容するか</param>
        /// <param name="rightBlank">抽出後ブランクを許容するか</param>
        /// <returns>抽出後の文字列</returns>
        public static (string left, string right)? ExtractSidesByDelimiter(string targetStr, char delimiter, bool leftBlank = true, bool rightBlank = true)
        {
            if (char.IsWhiteSpace(delimiter))
            {
                return null;
            }

            // 区切り文字が対象文字列に、複数含まれているかのチェック
            int ptnCount = Regex.Matches(targetStr, delimiter.ToString()).Count;

            if (ptnCount != 1)
            {
                // 区切り文字が対象文字列に複数存在するもしくは一つも存在しない場合は処理終了
                return null;
            }

            int delPosition = targetStr.IndexOf(delimiter);

            string leftStr = targetStr.Substring(0, delPosition);
            string rightStr = targetStr.Substring(delPosition +1);

            // 引数のBlankパラメータがfalseの場合、抽出対象がブランクの場合はそのままリターン
            if ((leftBlank == false && leftStr == string.Empty) || (rightBlank == false && rightStr == string.Empty))
            {
                return null;
            }

            return (leftStr, rightStr);
        }

        /// <summary>
        /// アプリケーションのリソースディレクトリ配下のテキストディレクトリに配置されているファイルを読み込み文字列として返します。
        /// </summary>
        /// <param name="fileName">取得対象ファイル名</param>
        /// <returns></returns>
        public static string GetTextOnResource(string fileName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Expense_Fair_Split.Resources.Text.{fileName}.txt"))
            {
                if (stream is null)
                {
                    return string.Empty;
                }
                else
                {
                    using var reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 渡された数値のみの文字列を金額形式に変換します。
        /// </summary>
        /// <param name="convertTarget">変換対象</param>
        /// <returns></returns>
        public static bool FormatNumberStringWithComma(ref string convertTarget)
        {
            try
            {
                convertTarget = int.Parse(convertTarget).ToString("N0");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 1～10の範囲の数値を0付きの二桁で文字列にして返します。11,12はそのまま
        /// </summary>
        /// <param name="convertMonth"></param>
        /// <returns></returns>
        public static string FormatMonthWithZero(int convertMonth)
        {
            const string FAILED_STR = "変換処理に失敗しました。";

            // 月の範囲の数字のみ対象とする
            if (convertMonth >= 1 && convertMonth <= 12)
            {
                string formatMonth = string.Empty;
                formatMonth = (convertMonth + 100).ToString();

                if (formatMonth.Length < 3) return FAILED_STR;

                return formatMonth.Substring(formatMonth.Length - 2);  // 右から二桁を返す
            }

            return FAILED_STR;
        }
    }
}

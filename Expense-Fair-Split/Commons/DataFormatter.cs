using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Commons
{
    public static partial class DataFormatter
    {
        public const string UNKNOWN = "Unknown";

        [GeneratedRegex(@"¥\s*(\d{1,3}(,\d{3})+|\d+)")]  // \から始まる数字（カンマなし、あり対応）
        private static partial Regex YenAmountRegex();

        /// <summary>
        /// 渡されたリストの金額形式の文字列とその一つ前の文字列を結合し一行にします。
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<string> ExtractItemsAndPrices(List<string> lines)
        {
            //const string UNKNOWN = "Unknown";  メンバ変数へ変更

            List<string> result = new List<string>();

            if (lines is null) return result;

            for (int i = 0; i < lines.Count -1; i++)
            {
                string current = lines[i];
                string next = lines[i + 1];

                // 金額形式にマッチしているか
                if (YenAmountRegex().IsMatch(current))
                {
                    // 一つ目が\だった場合は、商品名を取得出来ないので不明としてリストに登録し、そのまま次のループへ。
                    result.Add($"{UNKNOWN} - {YenAmountRegex().Match(current).Value.Replace(" ", "")}");
                    continue;
                }

                if (YenAmountRegex().IsMatch(next))
                {
                    // 一つ目がただの文字列、二つ目が\だった場合は、商品名 + 金額で登録する。

                    // 整形して結果に追加（空白除去と結合）
                    string item = current.Trim();
                    string price = YenAmountRegex().Match(next).Value.Replace(" ", "");
                    result.Add($"{item} - {price}");
                    i++; // 1つスキップ（次の行は既に処理済み）
                }
            }

            return result;
        }
    }
}

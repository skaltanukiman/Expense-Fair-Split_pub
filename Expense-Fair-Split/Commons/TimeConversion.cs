using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Commons
{
    public static class TimeConversion
    {
        /// <summary>
        /// DateTimeを UCT（世界協定時刻）とJST（日本標準時刻）の時間差を考慮した日時へ変換します。
        /// </summary>
        /// <param name="target">変換するパラメータ</param>
        /// <returns>
        ///     変換後の時間
        ///     targetがnullの場合はそのままnullを返します。
        /// </returns>
        public static DateTime? ConvertUtcToJst(DateTime? target)
        {
            if (target.HasValue)
            {
                return target.Value.AddHours(9);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// DateTimeを JST（日本標準時刻）とUCT（世界協定時刻）の時間差を考慮した日時へ変換します。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DateTime? ConvertJstToUtc(DateTime? target)
        {
            if (target.HasValue)
            {
                return target.Value.AddHours(-9);
            }
            else
            {
                return null;
            }
        }
    }
}

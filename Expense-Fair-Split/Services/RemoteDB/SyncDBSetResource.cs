using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.RemoteDB
{
    public static class SyncDBSetResource
    {
        public static readonly List<Type> syncDbSetType = [typeof(AccountData), typeof(User), typeof(BillingData), typeof(LogData)];  // ローカルDBからサーバーDBへどのDBSetを同期対象にするか

        // こちらを設定した場合、CompareDataService内の条件式も書き加える
        public static readonly List<Type> serverDbSetType = [typeof(AccountData), typeof(User), typeof(BillingData), typeof(LogData), typeof(MContactContent)];  // データ同期時、サーバーからどのDBSetデータを抽出するか
    }


    public static class GetDbSetIdName
    {
        /************************
         * 列名|列タイプ        *
         *                      *
         * 列タイプ             *
         * 1: 数値(int)         *
         * 2: 数値(long)        *
         *                      *
         ************************/

        public static string User => "Id|1";
        public static string AccountData => "AccId|1";
        public static string BillingData => "BillingNo|1";
        public static string LogData => "Id|2";
        public static string MContactContent => "Id|1";

        public static string Get(string propertyName)
        {
            PropertyInfo? propertyInfo = typeof(GetDbSetIdName).GetProperty(propertyName);
            return propertyInfo?.GetValue(null)?.ToString() ?? string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Commons
{
    public static class MappingStrResource
    {
        public static readonly string LoggedInUserId = "LoggedInUserId";
        public static readonly string DeleteFlagStr = "X";
        public static readonly string MediaTypeJsonStr = "application/json";
    }

    public static class ErrorMsgResource
    {
        public static readonly string UndefinedVM = "ViewModel is undefined";
        public static readonly string MissingNavPage = "NavigationPage is missing";
        public static readonly string ConstructorError = "constructor";

        public static string Get(string propName)
        {
            PropertyInfo? propertyInfo = typeof(ErrorMsgResource).GetProperty(propName);
            return propertyInfo?.GetValue(null)?.ToString() ?? string.Empty;
        }
    }

    public static class BillingStateStrResource
    {
        public static readonly string 取消 = "取消";
        public static readonly string 完了 = "完了";
        public static readonly string 承認待ち = "承認待ち";
        public static readonly string 未払い = "未払い";
        public static readonly string 確認中 = "確認中";
        public static readonly string 支払い待ち = "支払い待ち";
    }
}

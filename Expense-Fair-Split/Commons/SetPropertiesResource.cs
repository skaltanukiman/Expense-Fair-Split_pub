using Expense_Fair_Split.Models.PickerModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Commons
{
    public static class SetPropertiesResource
    {
        public static class BilingDataEntryProperties
        {
            public static readonly List<string> ratioPickerPropertyStr =
            [
                "請求割合を選択してください",
                "請求者100%負担",
                "9(請求者):1(受領者)",
                "8(請求者):2(受領者)",
                "7(請求者):3(受領者)",
                "6(請求者):4(受領者)",
                "5(請求者):5(受領者)",
                "4(請求者):6(受領者)",
                "3(請求者):7(受領者)",
                "2(請求者):8(受領者)",
                "1(請求者):9(受領者)",
                "受領者100%負担"
            ];
        }

        public static class BillingDataFilterProperties
        {
            public static readonly ObservableCollection<BillingDataFilterItem> billingDataFilter1ItemObCollection =
            [
                new BillingDataFilterItem{Id = 0, FilterName = "All"},
                new BillingDataFilterItem{Id = 1, FilterName = "請求分のみ"},
                new BillingDataFilterItem{Id = 2, FilterName = "受領分のみ"}
            ];

            public static readonly ObservableCollection<BillingDataFilterItem> billingDataFilter2ItemObCollection =
            [
                new BillingDataFilterItem{Id = 0, FilterName = "指定なし"},
                new BillingDataFilterItem{Id = 1, FilterName = "取引中"},
                new BillingDataFilterItem{Id = 2, FilterName = "完了分"},
                new BillingDataFilterItem{Id = 3, FilterName = "削除済み"}
            ];
        }

        public static class TypeSelection
        {
            public static Type? SelectByNumber(int selector)
            {
                switch (selector)
                {
                    case 1:
                        return typeof(int);
                    case 2:
                        return typeof(long);
                    default:
                        Debug.WriteLine($"{nameof(selector)}に対象範囲の数値を設定してください。");
                        return null;
                }
            }
        }
    }
}

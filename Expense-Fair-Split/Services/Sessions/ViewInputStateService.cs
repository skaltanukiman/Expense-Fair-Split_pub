using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Sessions
{
    public class ViewInputStateService
    {
        public BillingDataInquiryInputState BillingDataInquiry { get; set; } = new BillingDataInquiryInputState();
    }

    public class BillingDataInquiryInputState
    {
        public int? Filter1Index { get; set; } = null;
        public int? Filter2Index { get; set; } = null;
    }
}

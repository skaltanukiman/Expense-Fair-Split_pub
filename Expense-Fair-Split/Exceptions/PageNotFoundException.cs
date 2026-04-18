using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Exceptions
{
    public class PageNotFoundException : Exception
    {
        public PageNotFoundException() : base("ページが見つかりません。")
        {
        }

        public PageNotFoundException(string pageType) : base($"[{pageType}]が見つかりません。")
        {
        }

        public PageNotFoundException(Exception innerException) : base("ページが見つかりません。", innerException)
        {
        }

        public PageNotFoundException(string pageType, Exception innerException) : base($"[{pageType}]が見つかりません。", innerException)
        {
        }
    }
}

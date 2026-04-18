using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Exceptions
{
    public class NotFindItemException : Exception
    {
        public NotFindItemException() : base("アイテムが見つかりませんでした。")
        {
        }

        public NotFindItemException(string message) : base(message)
        {
        }

        public NotFindItemException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

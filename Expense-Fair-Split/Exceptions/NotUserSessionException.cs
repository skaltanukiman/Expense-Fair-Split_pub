using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Exceptions
{
    public class NotUserSessionException : Exception
    {
        public NotUserSessionException() : base("セッションからログイン情報が取得できませんでした。") 
        {
        }

        public NotUserSessionException(string message) : base(message) 
        {
        }

        public NotUserSessionException(string message, Exception innerException) : base(message, innerException)
        { 
        }
    }
}

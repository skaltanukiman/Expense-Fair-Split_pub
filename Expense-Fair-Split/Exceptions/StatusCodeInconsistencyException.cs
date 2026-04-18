using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Exceptions
{
    public class StatusCodeInconsistencyException : Exception
    {
        public StatusCodeInconsistencyException() : base("ステータスコードが不整合です。")
        {
        }

        public StatusCodeInconsistencyException(string message) : base(message) 
        {
        }

        public StatusCodeInconsistencyException(int code) : base($"ステータスコードが不整合です。StatusCode: {code}")
        {
        }

        public StatusCodeInconsistencyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

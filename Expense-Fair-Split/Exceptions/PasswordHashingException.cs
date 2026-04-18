using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Exceptions
{
    /// <summary>
    /// パスワードのハッシュ化失敗時例外
    /// </summary>
    public class PasswordHashingException : Exception
    {
        public PasswordHashingException() : base("パスワードのハッシュ化に失敗しました。")
        {
        }

        public PasswordHashingException(string message) : base(message) 
        {
        }

        public PasswordHashingException(string message, Exception innerException) : base(message, innerException) 
        {
        }
    }
}

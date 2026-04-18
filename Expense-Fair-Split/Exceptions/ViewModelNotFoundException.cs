using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Exceptions
{
    public class ViewModelNotFoundException : Exception
    {
        public ViewModelNotFoundException() : base("ViewModelが見つかりませんでした。") 
        {
        }

        public ViewModelNotFoundException(string vm) : base($"ViewModelが見つかりませんでした。({vm})")
        {
        }

        public ViewModelNotFoundException(Exception innerException) : base("ViewModelが見つかりませんでした。", innerException)
        {
        }

        public ViewModelNotFoundException(string vm, Exception innerException) : base($"ViewModelが見つかりませんでした。({vm})", innerException)
        {
        }
    }
}

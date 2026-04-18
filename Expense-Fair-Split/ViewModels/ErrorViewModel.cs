using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.ViewModels
{
    public class ErrorViewModel : Prism.Mvvm.BindableBase
    {
        public int? ErrorType { get; set; } = null;
        public int? ErrorCode { get; set; } = null;

        public bool ErrorInfoExists => ErrorType != null || ErrorCode != null;   // Error情報が渡されている場合のみTrue
    }
}

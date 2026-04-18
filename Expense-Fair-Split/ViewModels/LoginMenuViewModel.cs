using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.ViewModels
{
    public class LoginMenuViewModel : Prism.Mvvm.BindableBase
    {
        public string EmailAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        private string _errMsg = string.Empty;
        public string ErrMsg
        {
            get => _errMsg;
            set => SetProperty(ref _errMsg, value, nameof(ErrMsg));
        }

        private bool _showErrMsg = false;
        public bool ShowErrMsg
        {
            get => _showErrMsg;
            set => SetProperty(ref _showErrMsg, value, nameof(ShowErrMsg));
        }
    }
}

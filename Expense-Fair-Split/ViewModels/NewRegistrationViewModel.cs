using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.ViewModels
{
    public class NewRegistrationViewModel : Prism.Mvvm.BindableBase
    {
        public bool _isProcessing = false;

        #region UI Binding Properties
        public string UserName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;

        [Password(MinLength = 8)]
        public string PassWord { get; set; } = string.Empty;

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

        private string _processMsg = string.Empty;
        public string ProcessMsg
        {
            get => _processMsg;
            set => SetProperty(ref _processMsg, value, nameof(ProcessMsg));
        }

        private bool _showProcessMsg = false;
        public bool ShowProcessMsg
        {
            get => _showProcessMsg;
            set => SetProperty(ref _showProcessMsg, value, nameof(ShowProcessMsg));
        }
        #endregion

        /// <summary>
        /// ハッシュ化されたパスワードをフィールド上にセットする
        /// </summary>
        /// <param name="password">ハッシュ化対象文字列</param>
        /// <exception cref="Exception"></exception>
        public void SetHashPassword(string password)
        {
            string? hashedPassword = CommonUtil.HashPassword(password, out bool convSucceed);
            if (!convSucceed || hashedPassword is null)
            {
                throw new PasswordHashingException();
            }
            PassWord = hashedPassword;
        }
    }
}

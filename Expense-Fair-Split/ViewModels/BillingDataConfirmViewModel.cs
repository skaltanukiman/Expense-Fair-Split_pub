using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Models.PickerModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.ViewModels
{
    public class BillingDataConfirmViewModel : Prism.Mvvm.BindableBase
    {
        public bool _isProcessing = false;

        public BillingDataConfirmViewModel()
        {
            this.NowDate = CommonUtil.CreateTokyoJapanCurrentDateTime();
        }

        #region UI Binding Properties

        public string FromName { get; set; } = string.Empty;
        public ToPickerItem ToUserInfo { get; set; } = null!;
        public AccPickerItem AccInfo { get; set; } = null!;
        public DateTime NowDate { get; }
        public int RatioTypeCode { get; set; }
        public RatioPickerItem RatioInfo { get; set; } = null!;
        public int? TotalAmount { get; set; }
        public int AmountBilled { get; set; }
        public string Note { get; set; } = string.Empty;

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
    }
}

using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using System.Diagnostics;

namespace Expense_Fair_Split.Views;

public partial class BillingDataEntryPage : ContentPage
{
    private readonly IUserService _userService;
    private readonly ILogDataService _logDataService;
    private readonly UserSessionService _userSessionService;

    public BillingDataEntryPage()
	{
		InitializeComponent();
        var serviceProvider = App.Services;
        _userService = serviceProvider.GetRequiredService<IUserService>();
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();

        this.Loaded += async (_, _) =>
        {
            try
            {
                _vm = new BillingDataEntryViewModel(this);
                this.BindingContext = _vm;
                await _vm.PickersInitAsync();
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, $"{ErrorMsgResource.ConstructorError}|{nameof(BillingDataEntryPage)}", null);
                Application.Current!.MainPage = new ErrorPage();
            }
        };
    }
    BillingDataEntryViewModel? _vm;

    /// <summary>
    /// 請求確認画面への遷移および前準備
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnBillingDataConfirm(object sender, EventArgs e)
    {
        try
        {
            if (_vm is not null)
            {
                _vm.ErrMsg = "";
                bool showNextScreen = true;

                /*** 入力チェックStart ***/
                (string ErrMsg, bool IsChecked) result = CommonUtil.StrNullOrEmptyCheck(new List<string> { _vm.FromName, _vm.ToPickSelectedItem.Name, _vm.AccPickSelectedItem.AccName, _vm.RatioPickSelectedItem.RatioName });
                if (!result.IsChecked)
                {
                    _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                    _vm.ErrMsg += $"・{Properties.Resources.TargetStringEmpty}";
                    showNextScreen = false;
                }
                result = CommonUtil.RejectZeroOrNegative(new List<int> { _vm.TotalAmount.GetValueOrDefault() }, (int)EnumResource.ModeSelect.ZeroOrNegativeValue);
                if (!result.IsChecked)
                {
                    _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                    _vm.ErrMsg += $"・{Properties.Resources.TargetNumZeroOrNegative}";
                    showNextScreen = false;
                }
                result = CommonUtil.RejectZeroOrNegative(new List<int> { _vm.ToPickSelectedIndex, _vm.AccPickSelectedIndex, _vm.RatioPickSelectedIndex }, (int)EnumResource.ModeSelect.ZeroCheck);
                if (!result.IsChecked)
                {
                    _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                    _vm.ErrMsg += $"・{Properties.Resources.IncorrectSelectionItem}";
                    showNextScreen = false;
                }

                if (!showNextScreen)
                {
                    _vm.ShowErrMsg = true;
                    return;
                }
                /*** 入力チェックEnd ***/

                /*** 請求金額計算処理 ***/
                bool divSuccessFlg = false;
                _vm.AmountBilled = _vm.DivideTotalAmount(_vm.TotalAmount, ref divSuccessFlg, _vm.RatioPickSelectedItem.RatioCode);
                if (!divSuccessFlg)
                {
                    _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                    _vm.ErrMsg += $"・{Properties.Resources.BillingCalculationErr}";
                    _vm.ShowErrMsg = true;
                    return;
                }

                const int RATIOTYPECODE = 1;  // 現在は1のみなので定数で渡す（機能の拡張性を持つためにTypeCodeを持っておく）
                await this.Navigation.PushAsync(new BillingDataConfirmPage(new BillingDataConfirmViewModel { FromName = _vm.FromName, ToUserInfo = _vm.ToPickSelectedItem, AccInfo = _vm.AccPickSelectedItem, RatioTypeCode = RATIOTYPECODE, RatioInfo = _vm.RatioPickSelectedItem, TotalAmount = _vm.TotalAmount, AmountBilled = _vm.AmountBilled, Note = _vm.Note }));
            }
            else
            {
                throw new ViewModelNotFoundException(nameof(BillingDataEntryViewModel));
            }
        }
        catch (Exception ex) 
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(OnBillingDataConfirm), null);
            Application.Current!.MainPage = new ErrorPage();
        }
    }
}
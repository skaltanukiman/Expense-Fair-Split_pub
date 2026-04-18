using Expense_Fair_Split.Commons;
using Expense_Fair_Split.DTOs.ImageRecognition;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Ocr;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;

namespace Expense_Fair_Split.Views.ImageRecognition;

public partial class AfterImageRecognitionPage : ContentPage
{
    private readonly ILogDataService _logDataService;
    private readonly UserSessionService _userSessionService;
    private readonly BillingDataEntryViewModel _billingDataEntryViewModel;
    private readonly BeforeImageRecognitionPage _beforeImageRecognitionPage;

    public AfterImageRecognitionPage(BeforeImageRecognitionPage beforeImageRecognitionPage, BillingDataEntryViewModel billingDataEntryViewModel, PostVisionDto dto)
	{
		InitializeComponent();
        var serviceProvider = App.Services;
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
        _billingDataEntryViewModel = billingDataEntryViewModel;
        _beforeImageRecognitionPage = beforeImageRecognitionPage;

        this.Loaded += async (_, _) =>
        {
            try
            {
                _vm = new AfterImageRecognitionViewModel(_billingDataEntryViewModel, dto);
                this.BindingContext = _vm;
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, $"{ErrorMsgResource.ConstructorError}|{nameof(AfterImageRecognitionPage)}", null);
                Application.Current!.MainPage = new ErrorPage();
            }
        };
    }
    AfterImageRecognitionViewModel? _vm;

    /// <summary>
    /// チェックが付与された金額の足し引き
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ViewModelNotFoundException"></exception>
    /// <exception cref="NotFindItemException"></exception>
    private async void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            if (_vm is null) throw new ViewModelNotFoundException(nameof(AfterImageRecognitionViewModel));
            if (sender as CheckBox is not CheckBox checkBox) throw new NotFindItemException($"{nameof(CheckBox_CheckedChanged)}に渡されたオブジェクトが[{nameof(CheckBox)}]ではありません。");
            if (checkBox.BindingContext as AfterImageRecognitionViewDto is not AfterImageRecognitionViewDto viewDto) throw new NotFindItemException($"{nameof(CheckBox)}にバインドされているオブジェクトが[{nameof(AfterImageRecognitionViewDto)}]ではありません。");

            if (viewDto.IsTarget)
            {
                _vm.TotalAmount += viewDto.Amount;
            }
            else
            {
                _vm.TotalAmount += viewDto.Amount * -1;
            }
        }
        catch (Exception ex)
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(CheckBox_CheckedChanged), null);
            Application.Current!.MainPage = new ErrorPage();
        }
    }

    /// <summary>
    /// 必要な情報を渡した上で、請求入力画面へ戻る
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ViewModelNotFoundException"></exception>
    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm is null) throw new ViewModelNotFoundException(nameof(AfterImageRecognitionViewModel));

            if (_vm.TotalAmount < 0) _vm.TotalAmount = 0;

            _billingDataEntryViewModel.TotalAmount = _vm.TotalAmount;

            // 画像認識処理関連が終了したので、二画面分一気に戻す。
            await this.Navigation.PopModalAsync();
            await _beforeImageRecognitionPage.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(Button_Clicked), null);
            Application.Current!.MainPage = new ErrorPage();
        }
    }
}
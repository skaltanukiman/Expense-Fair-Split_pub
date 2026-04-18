using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using System.Reflection;

namespace Expense_Fair_Split.Views.ImageRecognition;

public partial class BeforeImageRecognitionPage : ContentPage
{
    private readonly ILogDataService _logDataService;
    private readonly UserSessionService _userSessionService;
    private readonly BillingDataEntryViewModel _billingDataEntryViewModel;

    private string? _cachedPopup = null;

    public BeforeImageRecognitionPage(BillingDataEntryViewModel billingDataEntryViewModel)
	{
		InitializeComponent();
        var serviceProvider = App.Services;
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
        _billingDataEntryViewModel = billingDataEntryViewModel;

        this.Loaded += async (_, _) =>
        {
            try
            {
                _vm = new BeforeImageRecognitionViewModel(_billingDataEntryViewModel);
                this.BindingContext = _vm;
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, $"{ErrorMsgResource.ConstructorError}|{nameof(BeforeImageRecognitionPage)}", null);
                Application.Current!.MainPage = new ErrorPage();
            }
        };
    }
    BeforeImageRecognitionViewModel? _vm;

    #region Event Method

    /// <summary>
    /// 戻るボタンを押下した際の処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnArrowLeftImage_ClickedAsync(object sender, EventArgs e)
    {
        await this.Navigation.PopModalAsync();
    }

    /// <summary>
    /// ボタン押下時の処理（カメラ起動前確認）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CameraActivationButtonAsync(object sender, EventArgs e)
    {
        string content = string.Empty;

        if (_vm is null)
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), Properties.Resources.NotViewModelExists, _userSessionService.UserId, nameof(CameraActivationButtonAsync), null);
            Application.Current!.MainPage = new ErrorPage();
        }

        // 既に実行中かを判定
        if (_vm!.GetProcessingStatus()) return;
        _vm.IsProcessing = true;
        _vm.ButtonIsEnable = false;

        try
        {
            try
            {
                if (string.IsNullOrEmpty(_cachedPopup))
                {
                    string getFileName = "camera_activation_confirmation";
                    content = CommonUtil.GetTextOnResource(getFileName);
                    _cachedPopup = content;
                }
                else
                {
                    content = _cachedPopup;
                }
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), $"ストリーム読み取り処理の中で問題が発生しました。({ex.Message})", _userSessionService.UserId, nameof(CameraActivationButtonAsync), null);
            }

            if (string.IsNullOrWhiteSpace(content)) content = "カメラを起動します。重要な情報は写さないでください。";

            bool answer = await this.DisplayAlert("", content, Properties.Resources.Ok, Properties.Resources.Cancel);
            if (!answer)
            {
                return;
            }

            try
            {
                // カメラ起動
                bool ocrSuccess = await _vm!.StartOCRFunc();

                if (!ocrSuccess || _vm.VisionDto is null) return;  // 上記処理内でエラーメッセージを表示しているのでそのままリターン

                // 画像認識成功後、認識処理後ページへ遷移
                await this.Navigation.PushModalAsync(new AfterImageRecognitionPage(this, _billingDataEntryViewModel, _vm.VisionDto));
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(CameraActivationButtonAsync), null);
                Application.Current!.MainPage = new ErrorPage();
            }
        }
        finally
        {
            _vm!.IsProcessing = false;
            _vm.ButtonIsEnable = true;
        }
    }

    #endregion
}
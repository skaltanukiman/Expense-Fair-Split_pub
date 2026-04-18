using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Api;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Expense_Fair_Split.Views;

public partial class BillingDataConfirmPage : ContentPage
{
    private readonly IUserService _userService;
    private readonly UserSessionService _userSessionService;
    private readonly IBillingDataService _billingDataService;
    private readonly ILogDataService _logDataService;
    private readonly ApiClient _apiClient;

    public BillingDataConfirmPage(BillingDataConfirmViewModel billingData)
	{
		InitializeComponent();
        var serviceProvider = App.Services;
        _userService = serviceProvider.GetRequiredService<IUserService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
        _billingDataService = serviceProvider.GetRequiredService<IBillingDataService>();
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _apiClient = serviceProvider.GetRequiredService<ApiClient>();

        this.Loaded += (_, _) =>
		{
			_vm = billingData;
			this.BindingContext = _vm;
		};
	}
	BillingDataConfirmViewModel? _vm;

	private async void PopBillingDataEntry(object sender, EventArgs e)
	{
		// ここに戻るボタンの処理
		await this.Navigation.PopAsync();
	}

    private async void OnPublishing(object sender, EventArgs e)
    {
        try
        {
            if (_vm is not null)
            {
                if (_vm._isProcessing)
                {
                    _vm.ProcessMsg = $"・{Properties.Resources.isProcessing}";
                    _vm.ShowProcessMsg = true;
                    return;
                }

                try
                {
                    _vm._isProcessing = true;

                    // ここに発行ボタンの処理
                    bool answer = await DisplayAlert("確認", "画面の内容で請求を行ってもよろしいですか？", "OK", "Cancel");
                    if (answer)
                    {
                        _vm.ErrMsg = string.Empty;
                        _vm.ShowErrMsg = false;

                        // 請求データ登録&画面遷移処理
                        if (_userSessionService is null || _userSessionService.UserId == -1)
                        {
                            throw new NotUserSessionException();
                        }

                        BillingData createData = new BillingData()
                        {
                            BillingDate = _vm.NowDate,
                            AccountCode = _vm.AccInfo.Id,
                            RatioTypeCode = _vm.RatioTypeCode,
                            RatioCode = _vm.RatioInfo.RatioCode,
                            FromUserCode = _userSessionService.UserId,
                            ToUserCode = _vm.ToUserInfo.Id,
                            TotalAmount = _vm.TotalAmount ??= 0,
                            BillingAmount = _vm.AmountBilled,
                            StatusCode = (int)EnumResource.StatusCode.BillingStart,
                            Note = _vm.Note
                        };

                        // SQLite（ローカル）への登録
                        try
                        {
                            await _billingDataService.CreateBillingDataAsync(createData);
                        }
                        catch (DbUpdateException ex)
                        {
                            Debug.WriteLine(ex.Message);
                            _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                            _vm.ErrMsg += $"・{Properties.Resources.DataInsertFailed}";
                            _vm.ShowErrMsg = true;
                            return;
                        }

                        // PostgreSQLへの登録
                        try
                        {
                            HttpResponseMessage response = await _apiClient.PostAsync("api/BillingData", createData);

                            if (response.IsSuccessStatusCode)
                            {
                                // DBへの登録成功時の処理
                                createData.IsSynced = true;  // PostgreSQLへの登録成功時は同期フラグを同期済みに変更
                                await _billingDataService.UpdateBillingDataAsync(createData);
                            }
                            else
                            {
                                throw new Exception("PostgreSQL への明細データの登録に失敗しました。");
                            }
                        }
                        catch (Exception)
                        {
                            Debug.WriteLine("PostgreSQL への明細データの登録に失敗しました。ローカルデータを削除します。");

                            await _billingDataService.DeleteBillingDataAsync(createData.BillingNo);  // DBへの書き込み失敗時、ローカルに登録した明細データを消す

                            _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                            _vm.ErrMsg += $"・{Properties.Resources.DataInsertFailed}";
                            _vm.ShowErrMsg = true;
                            return;
                        }

                        // 画面遷移の準備
                        await Shell.Current.GoToAsync("//BillingDataInquiry");
                        await this.Navigation.PopAsync();  // 画面遷移後に、同登録内容の登録処理を再実行できないように一つ前のページに戻る
                    }
                }
                finally
                {
                    _vm._isProcessing = false;
                    _vm.ShowProcessMsg = false;
                }
            }
            else
            {
                throw new ViewModelNotFoundException(nameof(BillingDataConfirmViewModel));
            }
        }
        catch (Exception ex)
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(OnPublishing), null);
            Application.Current!.MainPage = new ErrorPage();
        }
    }
}
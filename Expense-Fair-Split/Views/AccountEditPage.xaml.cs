using Expense_Fair_Split.Commons;
using Expense_Fair_Split.DTOs.AccountData;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Api;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using Syncfusion.Maui.DataGrid;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http.Json;

namespace Expense_Fair_Split.Views;

public partial class AccountEditPage : ContentPage
{
    private readonly IAccountDataService _accountDataService;
    private readonly ILogDataService _logDataService;
    private readonly UserSessionService _userSessionService;
    private readonly ApiClient _apiClient;

    public AccountEditPage()
	{
		InitializeComponent();
        var serviceProvider = App.Services;
        _accountDataService = serviceProvider.GetRequiredService<IAccountDataService>();
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
        _apiClient = serviceProvider.GetRequiredService<ApiClient>();

        this.Loaded += async (_, _) =>
		{
			try
			{
                _vm = new AccountEditViewModel();
                this.BindingContext = _vm;
                
                await _vm.SetAccountDataListAsync();
            }
			catch (Exception ex)
			{
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, $"{ErrorMsgResource.ConstructorError}|{nameof(AccountEditPage)}", null);
                Application.Current!.MainPage = new ErrorPage();
            }
        };
	}
	AccountEditViewModel? _vm;

    /// <summary>
    /// グリッドビュー内のセルの変更が確定された時の処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnDataGridCellEndEdit(object sender, DataGridCurrentCellEndEditEventArgs e)
    {
        try
        {
            if (_vm is not null)
            {
                _vm.ErrMsg = string.Empty;
                _vm.ShowErrMsg = false;

                if (sender is SfDataGrid dataGrid)
                {
                    // 登録予定の値がnullや空白でないかをチェック
                    if (string.IsNullOrWhiteSpace(e.NewValue?.ToString()))
                    {
                        _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                        _vm.ErrMsg += $"・{Properties.Resources.CanNotAccNameEmpty}";
                        _vm.ShowErrMsg = true;
                        e.Cancel = true;
                        return;
                    }

                    // 変更前の値と編集後の値が一緒の場合はそのままリターン
                    if ((e.OldValue?.ToString() is not null && e.NewValue.ToString() is not null) && (e.OldValue.ToString() == e.NewValue.ToString()))
                    {
                        e.Cancel = true;
                        Debug.WriteLine("stringEqualCancel");
                        return;
                    }

                    int dataRowIndex = e.RowColumnIndex.RowIndex - 1;  // データ部の行番号（上から0）

                    // 編集前の値が入った編集対象レコードを取得
                    AccountDataGridViewDto? editItem = dataGrid.View?.Records[dataRowIndex]?.Data as AccountDataGridViewDto;
                    if (editItem is null)
                    {
                        throw new NotFindItemException($"[{nameof(editItem)}]が取得できませんでした。");
                    }

                    // 変更予定の勘定名がDBに登録されていないかチェック&更新用にAccountDataインスタンスを生成
                    AccountData? checkAccountData = await _accountDataService.GetAccountDataByAccountNameAsync((string)e.NewValue);
                    if (checkAccountData is not null)
                    {
                        _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                        _vm.ErrMsg += $"・この勘定名は既に登録されています。({(string)e.NewValue})";
                        _vm.ShowErrMsg = true;
                        e.Cancel = true;
                        return;
                    }

                    // 更新対象のデータを取得
                    AccountData? updateData = await _accountDataService.GetAccountDataAsync(editItem.AccId);
                    if (updateData is null)
                    {
                        _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                        _vm.ErrMsg += $"・{Properties.Resources.UpdateFailed}";
                        _vm.ShowErrMsg = true;
                        e.Cancel = true;
                        return;
                    }

                    // 更新対象プロパティの値を変更&ロールバックのために元の値も保持しておく
                    string rollbackAccName = updateData.AccName;
                    int? rollbackUpdateUserId = updateData.UpdateUserId is not null ? updateData.UpdateUserId : null;
                    DateTime? rollbackUpdateDate = updateData.UpdateDate is not null ? updateData.UpdateDate : null;

                    updateData.AccName = (string)e.NewValue;
                    updateData.UpdateUserId = _userSessionService.UserId;
                    updateData.UpdateDate = CommonUtil.CreateTokyoJapanCurrentDateTime();

                    // UIロールバックのための値もコピーしておく
                    string viewRollbackAccName = editItem.AccName;
                    string viewRollbackUpdateUser = editItem.UpdateUser;
                    DateTime? viewRollbackUpdateDate = editItem.UpdateDate;

                    try
                    {
                        // ローカルのデータを更新
                        await _accountDataService.UpdateAccountDataAsync(updateData);
                        Debug.WriteLine("ローカル更新処理成功");

                        // 画面に戻った際のUI更新のためDTOに値を移す
                        editItem.AccName = updateData.AccName;
                        editItem.UpdateUser = _userSessionService.UserName;
                        editItem.UpdateDate = updateData.UpdateDate;
                    }
                    catch (Exception ex)
                    {
                        _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                        _vm.ErrMsg += $"・{Properties.Resources.UpdateFailed}";
                        _vm.ShowErrMsg = true;
                        e.Cancel = true;
                        return;
                    }

                    // PostgreSQLのデータを更新
                    try
                    {
                        var response = await _apiClient.PutAsync($"api/AccountData/{updateData.AccId}", updateData);
                        if (response.IsSuccessStatusCode)
                        {
                            updateData.IsSynced = true;  // PostgreSQLへの登録成功時は同期フラグを同期済みに変更
                            await _accountDataService.UpdateAccountDataAsync(updateData);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("PostgreSQLへの更新処理中に予期せぬエラーが発生しました。更新したパラメータをロールバックします。");

                        updateData.AccName = rollbackAccName;
                        updateData.UpdateUserId = rollbackUpdateUserId;
                        updateData.UpdateDate = rollbackUpdateDate;
                        await _accountDataService.UpdateAccountDataAsync(updateData);  // DBへの書き込み失敗時、ローカルに登録したデータの更新分を取り消す。

                        // なぜかe.Cancelが反映されず、UIの表示がロールバックされないので、手動であらかじめ保持しておいた変数から変更する
                        editItem.AccName = viewRollbackAccName;
                        editItem.UpdateUser = viewRollbackUpdateUser;
                        editItem.UpdateDate = viewRollbackUpdateDate;

                        _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                        _vm.ErrMsg += $"・{Properties.Resources.ServerCommunicationError}";
                        _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                        _vm.ErrMsg += $"・{Properties.Resources.UpdateFailed}";
                        _vm.ShowErrMsg = true;
                        e.Cancel = true;

                        return;
                    }
                }
                else
                {
                    throw new NotFindItemException($"[{nameof(SfDataGrid)}]が取得できませんでした。");
                }
            }
            else
            {
                throw new ViewModelNotFoundException(nameof(AccountEditViewModel));
            }
        }
        catch (Exception ex)
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(OnDataGridCellEndEdit), null);
            Application.Current!.MainPage = new ErrorPage();
        }
    }
}
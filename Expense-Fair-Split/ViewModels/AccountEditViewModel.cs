using Expense_Fair_Split.Commons;
using Expense_Fair_Split.DTOs.AccountData;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Extensions;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Api;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.Views.Error;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace Expense_Fair_Split.ViewModels
{
    public class AccountEditViewModel : Prism.Mvvm.BindableBase
    {
        public DelegateCommand AddAccCommand { get; }
        private readonly UserSessionService _userSessionService;
        private readonly IAccountDataService _accountDataService;
        private readonly IUserService _userService;
        private readonly ILogDataService _logDataService;
        private readonly ApiClient _apiClient;
        private bool _isProcessing = false;

        public AccountEditViewModel() 
        {
            var serviceProvider = App.Services;
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _accountDataService = serviceProvider.GetRequiredService<IAccountDataService>();
            _userService = serviceProvider.GetRequiredService<IUserService>();
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _apiClient = serviceProvider.GetRequiredService<ApiClient>();
            AddAccCommand = new DelegateCommand(async () => await AddAccount(), CanExecuteAddAccount)
                             .ObservesProperty(() => AdditionalAccount);
        }

        #region UI Binding Properties

        private ObservableCollection<AccountDataGridViewDto> _accountDataList = new ObservableCollection<AccountDataGridViewDto>();
        public ObservableCollection<AccountDataGridViewDto> AccountDataList
        {
            get => _accountDataList;
            set => SetProperty(ref _accountDataList, value, nameof(AccountDataList));
        }

        private string _AdditionalAccount = string.Empty;
        public string AdditionalAccount
        {
            get => _AdditionalAccount;
            set => SetProperty(ref _AdditionalAccount, value, nameof(AdditionalAccount));
        }

        // errMsg1
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

        // errMsg2
        private string _errMsg2 = string.Empty;
        public string ErrMsg2
        {
            get => _errMsg2;
            set
            {
                if (SetProperty(ref _errMsg2, value, nameof(ErrMsg2)))
                {
                    ShowErrMsg2 = _errMsg2 != string.Empty;
                }
            }
        }

        private bool _showErrMsg2 = false;
        public bool ShowErrMsg2
        {
            get => _showErrMsg2;
            set => SetProperty(ref _showErrMsg2, value, nameof(ShowErrMsg2));
        }

        #endregion

        #region SetProperties Method

        /// <summary>
        /// 勘定リストをプロパティにセットする
        /// </summary>
        /// <returns></returns>
        public async Task SetAccountDataListAsync()
        {
            List<AccountData> accountDataList = (await _accountDataService.GetAllAccountDataAsync()).ToList();
            List<User> userList = (await _userService.GetAllUsersAsync()).ToList();
            if (accountDataList is null || accountDataList.Count == 0) 
            {
                // 取得できなかった場合（まだ登録がない等）はリターン
                return;
            }
            else
            {
                // (Key: user => ID, Value: user => Name)
                Dictionary<int, string> userDict = userList.ToDictionary(user => user.Id, user => user.Name);

                var result = accountDataList.Select(accountData =>
                {
                    string createName = userDict.TryGetValue(accountData.CreateUserId, out string? createUserName) ? createUserName : string.Empty;
                    string updateName = userDict.TryGetValue(accountData.UpdateUserId ?? -1, out string? updateUserName) ? updateUserName : string.Empty;  // nullの場合、-1に変換して照合する（-1のIDを持つユーザーは存在しない）

                    AccountDataGridViewDto accountDataGridViewDto = new AccountDataGridViewDto(this)
                    {
                        AccId = accountData.AccId,
                        AccName = accountData.AccName,
                        CreateUser = createName,
                        CreateDate = accountData.CreateDate,
                        UpdateUser = updateName,
                        UpdateDate = accountData.UpdateDate,
                        DelFlg = accountData.DelFlg
                    };
                    return accountDataGridViewDto;
                }).ToList();

                if (result is null || result.Count == 0)
                {
                    throw new NotFindItemException($"[{nameof(result)}]が取得できませんでした。");
                }
                else 
                {
                    foreach (AccountDataGridViewDto item in result) AccountDataList.Add(item);
                }
            }
        }

        #endregion

        #region CanExecuteMethod

        private bool CanExecuteAddAccount()
        {
            return !string.IsNullOrEmpty(AdditionalAccount) && !_isProcessing;  // 追加勘定が入力されている&処理未実行時のみ押下可能
        }

        #endregion

        #region CommandMethod

        /// <summary>
        /// マスタに勘定を追加する。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotUserSessionException"></exception>
        private async Task AddAccount()
        {
            if (_isProcessing) return;
            _isProcessing = true;
            AddAccCommand.RaiseCanExecuteChanged();  // 実行処理中はボタンを無効化

            try
            {
                ErrMsg = string.Empty;
                ShowErrMsg = false;

                if (string.IsNullOrWhiteSpace(AdditionalAccount))
                {
                    ErrMsg += $"・{Properties.Resources.ItemsEmpty}（勘定）";
                    ShowErrMsg = true;
                    return;
                }
                string addAccountStr = AdditionalAccount.Trim();

                if (await _accountDataService.GetAccountDataByAccountNameAsync(addAccountStr) is not null)
                {
                    ErrMsg += $"・{Properties.Resources.TheAccountIsAlreadyRegistered}";
                    ShowErrMsg = true;
                    return;
                }

                int createUserId = _userSessionService.UserId;
                if (createUserId == -1)
                {
                    throw new NotUserSessionException();
                }

                AccountData addAccData = new AccountData { AccName = addAccountStr, CreateUserId = createUserId, CreateDate = CommonUtil.CreateTokyoJapanCurrentDateTime() };
                try
                {
                    // SQLite（ローカル）への登録
                    await _accountDataService.CreateAccountDataAsync(addAccData);

                    // 勘定追加後に追加内容を含めるため、UIを更新する
                    AccountDataGridViewDto newDto = new AccountDataGridViewDto(this)
                    {
                        AccId = addAccData.AccId,
                        AccName = addAccData.AccName,
                        CreateUser = _userSessionService.UserName,
                        CreateDate = addAccData.CreateDate,
                        DelFlg = addAccData.DelFlg
                    };

                    AccountDataList.Add(newDto);
                    AccountDataList.SortById(accountData => accountData.AccId);

                    // 登録完了した際のメッセージを表示する　後日追加


                }
                catch (ArgumentNullException ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
                catch (DbUpdateException ex)
                {
                    Debug.WriteLine(ex.Message);
                    ErrMsg += $"・勘定の{Properties.Resources.DataInsertFailed}";
                    ShowErrMsg = true;
                    return;
                }

                // PostgreSQLへの登録
                try
                {
                    var response = await _apiClient.PostAsync("api/AccountData", addAccData);
                    if (response.IsSuccessStatusCode)
                    {
                        addAccData.IsSynced = true;  // PostgreSQLへの登録成功時は同期フラグを同期済みに変更
                        await _accountDataService.UpdateAccountDataAsync(addAccData);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("PostgreSQLへの登録中に予期せぬエラーが発生しました。ローカルのデータを削除します。");
                    await _accountDataService.DeleteAccountDataAsync(addAccData.AccId);  // DBへの書き込み失敗時、ローカルに登録したデータを消す

                    ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                    ErrMsg += $"・{Properties.Resources.ServerCommunicationError}";
                    ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                    ErrMsg += $"・{Properties.Resources.CancelRegistration}";

                    ShowErrMsg = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(AddAccount), null);
                Application.Current!.MainPage = new ErrorPage();
            }
            finally
            {
                _isProcessing = false;
                AddAccCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion
    }
}

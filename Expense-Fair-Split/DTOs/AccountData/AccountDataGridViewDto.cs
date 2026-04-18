using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Api;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;

namespace Expense_Fair_Split.DTOs.AccountData
{
    public class AccountDataGridViewDto : Prism.Mvvm.BindableBase, IEditableObject
    {
        private readonly IAccountDataService _accountDataService;
        private readonly ILogDataService _logDataService;
        private readonly UserSessionService _userSessionService;
        private readonly ApiClient _apiClient;
        private readonly AccountEditViewModel _vm;

        public AccountDataGridViewDto(AccountEditViewModel vm)
        {
            var serviceProvider = App.Services;
            _accountDataService = serviceProvider.GetRequiredService<IAccountDataService>();
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _apiClient = serviceProvider.GetRequiredService<ApiClient>();
            _vm = vm;
        }

        #region UI Binding Properties

        private int _accId;
        public int AccId
        {
            get => _accId;
            set => SetProperty(ref _accId, value, nameof(AccId));
        }

        private string _accName = null!;
        public string AccName
        {
            get => _accName;
            set => SetProperty(ref _accName, value, nameof(AccName));
        }

        private string _createUser = string.Empty;
        public string CreateUser
        {
            get => _createUser;
            set => SetProperty(ref _createUser, value, nameof(CreateUser));
        }

        private DateTime _createDate;
        public DateTime CreateDate
        {
            get => _createDate;
            set => SetProperty(ref _createDate, value, nameof(CreateDate));
        }

        private string _updateUser = string.Empty;
        public string UpdateUser
        {
            get => _updateUser;
            set => SetProperty(ref _updateUser, value, nameof(UpdateUser));
        }

        private DateTime? _updateDate = null;
        public DateTime? UpdateDate
        {
            get => _updateDate;
            set => SetProperty(ref _updateDate, value, nameof(UpdateDate));
        }

        private bool _delFlg;
        public bool DelFlg
        {
            get => _delFlg;
            set
            {
                if (SetProperty(ref _delFlg, value, nameof(DelFlg)))
                {
                    Debug.WriteLine(_delFlg);
                    DelFlgChangedDBUpdate();
                }
            }
        }

        #endregion

        #region CRUD Method

        /// <summary>
        /// DelFlgの値とDBに保持している値に差異があれば更新を行います。
        /// </summary>
        private async void DelFlgChangedDBUpdate()
        {
            bool rollbackUIDelFlg = !_delFlg;  // 更新失敗時にUIを更新前に戻すためのロールバック値を格納（DelFlgプロパティの反対となる真偽値）

            bool? originalDelFlg = null;       // 更新失敗時にDBデータを更新前に戻すためのロールバック値を格納
            int? originalUpdateUserId = null;
            DateTime? originalUpdateDate = _updateDate;


            Expense_Fair_Split.Models.AccountData? updateData = await _accountDataService.GetAccountDataAsync(AccId);

            // ローカルDBへの登録
            try
            {
                if (updateData is null) throw new NotFindItemException($"[{nameof(DelFlgChangedDBUpdate)}]内でupdate対象の[{nameof(AccountData)}]が見つかりませんでした。");
                if (updateData.DelFlg == _delFlg) return;  // 更新対象のフラグと更新値が等しい場合はそのままリターン

                _vm.ErrMsg2 = string.Empty;

                // ロールバック値の代入
                originalDelFlg = updateData.DelFlg;
                originalUpdateUserId = updateData.UpdateUserId;

                updateData.DelFlg = _delFlg;
                updateData.UpdateDate = CommonUtil.CreateTokyoJapanCurrentDateTime();
                updateData.UpdateUserId = _userSessionService.UserId;

                await _accountDataService.UpdateAccountDataAsync(updateData);

                UpdateUser = _userSessionService.UserName;  // DB更新成功後に画面をリフレッシュする
                UpdateDate = updateData.UpdateDate;
            }
            catch (NotFindItemException ex)
            {
                Debug.WriteLine(ex.Message);
                _vm.ErrMsg2 = $"・{Properties.Resources.AccountDelFlgUpdateFailed}";
                DelFlg = rollbackUIDelFlg;
                return;
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);
                _vm.ErrMsg2 = $"・{Properties.Resources.AccountDelFlgUpdateFailed}";
                updateData!.DelFlg = originalDelFlg!.Value;  // 画面のDelFlgを更新するとメソッドが再起するので先にDBの値をロールバックする
                updateData!.UpdateDate = originalUpdateDate;
                updateData!.UpdateUserId = originalUpdateUserId;

                DelFlg = rollbackUIDelFlg;
                return;
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(DelFlgChangedDBUpdate), null);
                Application.Current!.MainPage = new ErrorPage();
            }

            // PostgreSQLのデータを更新
            try
            {
                var response = await _apiClient.PutAsync($"api/AccountData/{updateData!.AccId}", updateData);
                if (response.IsSuccessStatusCode)
                {
                    updateData.IsSynced = true;  // PostgreSQLへの更新成功時は同期フラグを同期済みに変更
                    await _accountDataService.UpdateAccountDataAsync(updateData);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("PostgreSQLへの更新処理中に予期せぬエラーが発生しました。");

                updateData!.IsSynced = false;  // PostgreSQLへの更新失敗時は同期フラグを未同期に変更
                await _accountDataService.UpdateAccountDataAsync(updateData);
            }
        }

        #endregion

        #region PropertyChanged implementation

        private Dictionary<string, object?>? storedValues;


        public void BeginEdit()
        {
            this.storedValues = this.BackUp();
        }

        public void CancelEdit()
        {
            if (this.storedValues == null)
                return;

            foreach (var item in this.storedValues)
            {
                var itemProperties = this.GetType().GetTypeInfo().DeclaredProperties;
                var pDesc = itemProperties.FirstOrDefault(p => p.Name == item.Key);
                if (pDesc != null)
                    pDesc.SetValue(this, item.Value);
            }
        }

        public void EndEdit()
        {
            if (this.storedValues != null)
            {
                this.storedValues.Clear();
                this.storedValues = null;
            }
            Debug.WriteLine("End Edit Called");
        }

        protected Dictionary<string, object?> BackUp()
        {
            var dictionary = new Dictionary<string, object?>();
            var itemProperties = this.GetType().GetTypeInfo().DeclaredProperties;
            foreach (var pDescriptor in itemProperties)
            {
                if (pDescriptor.CanWrite)
                    dictionary.Add(pDescriptor.Name, pDescriptor.GetValue(this));
            }
            return dictionary;
        }
        #endregion
    }
}

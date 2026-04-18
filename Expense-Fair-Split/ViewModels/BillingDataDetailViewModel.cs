using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Api;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.Views;
using Expense_Fair_Split.Views.Error;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.ViewModels
{
    public class BillingDataDetailViewModel : Prism.Mvvm.BindableBase
    {
        private readonly IBillingDataService _billingDataService;
        private readonly UserSessionService _userSessionService;
        private readonly ILogDataService _logDataService;
        private readonly IAlertService _alertService;
        private readonly ApiClient _apiClient;
        private BillingDataDetailPage? _billingDataDetailPage;

        public DelegateCommand ApproveActionCommand { get; }
        public DelegateCommand DenyActionCommand { get; }
        public DelegateCommand SendPaymentCompleteCommand {  get; }
        public DelegateCommand RenderSplitAmountsCommand { get; }

        private BillingData? OperationData;
        private bool isProcessing = false;

        public BillingDataDetailViewModel()
        {
            var serviceProvider = App.Services;
            _billingDataService = serviceProvider.GetRequiredService<IBillingDataService>();
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _alertService = serviceProvider.GetRequiredService<IAlertService>();
            _apiClient = serviceProvider.GetRequiredService<ApiClient>();

            ApproveActionCommand = new DelegateCommand(async () => await OnApproveButtonClicked(), CanExecuteButton);
            DenyActionCommand = new DelegateCommand(async () => await OnDenyButtonClicked(), CanExecuteButton);
            SendPaymentCompleteCommand = new DelegateCommand(async () => await OnSendPaymentComplete(), CanExecuteButton);
            RenderSplitAmountsCommand = new DelegateCommand(async () => await RenderSplitAmounts());
        }

        #region UI Binding Properties

        public int BillingNo { get; set; }
        public DateTime BillingDate { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public int FromUserCode { get; set; }
        public string FromUserName { get; set; } = string.Empty;
        public int ToUserCode { get; set; }
        public string ToUserName { get; set; } = string.Empty;
        public string RatioText { get; set; } = string.Empty;
        public int TotalAmount { get; set; }
        public int AmountBilled { get; set; }
        public int StatusCode { get; set; }

        private string _statusDisplayViewStr = string.Empty;
        public string StatusDisplayViewStr
        {
            get => _statusDisplayViewStr;
            set
            {
                if (SetProperty(ref _statusDisplayViewStr, value, nameof(StatusDisplayViewStr)))
                {
                    IsApprovalPending = _statusDisplayViewStr == "承認待ち";  // 値が「承認待ち」の場合、Trueにそれ以外の場合Falseに
                    IsAwaitingPayment = _statusDisplayViewStr == "未払い";
                }
            }
        }

        public string Note { get; set; } = string.Empty;
        public string DeleteFlag { get; set; } = string.Empty;

        // Trueの場合: 画面に表示、Falseの場合: 非表示
        private bool _isApprovalPending;
        public bool IsApprovalPending
        {
            get => _isApprovalPending;
            set => SetProperty(ref _isApprovalPending, value, nameof(IsApprovalPending));
        }

        private bool _isAwaitingPayment;
        public bool IsAwaitingPayment
        {
            get => _isAwaitingPayment;
            set => SetProperty(ref _isAwaitingPayment, value, nameof(IsAwaitingPayment));
        }

        private string _errMsg = string.Empty;
        public string ErrMsg
        {
            get => _errMsg;
            set
            {
                if (SetProperty(ref _errMsg, value, nameof(ErrMsg)))
                {
                    ShowErrMsg = _errMsg != string.Empty;
                }
            }
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
            set
            {
                if (SetProperty(ref _processMsg, value, nameof(ProcessMsg)))
                {
                    ShowProcessMsg = _processMsg != string.Empty;
                }
            }
        }

        private bool _showProcessMsg = false;
        public bool ShowProcessMsg
        {
            get => _showProcessMsg;
            set => SetProperty(ref _showProcessMsg, value, nameof(ShowProcessMsg));
        }

        #endregion

        #region CanExecuteMethod

        private bool CanExecuteButton()
        {
            return !isProcessing;  // 処理未実行時のみ押下可能
        }

        #endregion

        #region CRUD method

        /// <summary>
        /// 引数で渡されたBillingDataでDBを更新します。失敗した場合、StatusCodeの値を変更前の値にロールバックします。
        /// </summary>
        /// <param name="operationData">更新対象のデータ</param>
        /// <param name="rollbackStatusCode">更新に失敗した時のロールバック用の値</param>
        ///  <param name="optionalRollbackDelFlg">(省略可能)更新に失敗した時のロールバック用の値</param>
        /// <returns>
        ///     true: 更新成功
        ///     false: 更新失敗
        /// </returns>
        private async Task<bool> UpdateBillingDataByOperationData(BillingData operationData, int rollbackStatusCode, string? optionalRollbackDelFlg = null)
        {
            // SQLite（ローカル）の更新
            try
            {
                await _billingDataService.UpdateBillingDataAsync(operationData);
            }
            catch (Exception)
            {
                StatusCode = rollbackStatusCode;
                operationData.StatusCode = rollbackStatusCode;

                if (optionalRollbackDelFlg is not null) 
                {
                    DeleteFlag = optionalRollbackDelFlg;
                    operationData.DeleteFlag = optionalRollbackDelFlg;
                }

                ErrMsg += $"・{Properties.Resources.UpdateFailed}";
                return false;
            }

            // PostgreSQLの更新
            try
            {
                HttpResponseMessage response = await _apiClient.PutAsync($"api/BillingData/{operationData.BillingNo}", operationData);
                if (response.IsSuccessStatusCode)
                {
                    // DBへの登録成功時の処理
                    operationData.IsSynced = true;  // PostgreSQLへの更新成功時は同期フラグを同期済みに変更
                    await _billingDataService.UpdateBillingDataAsync(operationData);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("明細データのPostgreSQLへの更新処理に失敗しました。");
                operationData.IsSynced = false;  // PostgreSQLへの更新失敗時は同期フラグを未同期に変更
                await _billingDataService.UpdateBillingDataAsync(operationData);
            }

            return true;
        }

        #endregion

        #region Validate method

        /// <summary>
        /// ビューモデルのプロパティとして持つ明細NoとBillingDataオブジェクトの持つ明細Noが一致しているか検証します。
        /// </summary>
        /// <returns>
        ///     true: 一致
        ///     false: 不一致
        /// </returns>
        private bool CheckBillingNoWithObject()
        {
            if (OperationData is null || OperationData.BillingNo != this.BillingNo)
            {
                ErrMsg = $"・{Properties.Resources.ErrorDuringProcessing}";
                return false;
            }
            return true;
        }

        /// <summary>
        /// ビューモデルのプロセス判定変数を参照し、処理実行中であればエラーメッセージとtrue(実行中)、それ以外はfalseを返します。
        /// </summary>
        /// <returns>
        ///     true: 実行中
        ///     false: 待機中
        /// </returns>
        private bool GetProcessingStatus()
        {
            if (isProcessing)
            {
                ProcessMsg = $"・{Properties.Resources.isProcessing}";
                return true;
            }
            return false;
        }

        #endregion

        #region SetProperties method

        /// <summary>
        /// ビューモデルが持つStatusDisplayViewStrに条件に応じた値をセットします。
        /// </summary>
        public void SetDisplayViewStatus()
        {
            bool? isPayer = CommonUtil.CheckUserRoleInBilling(FromUserCode, ToUserCode);
            StatusDisplayViewStr = CommonUtil.GetBillingStatusMessage(isPayer, StatusCode);
        }

        /// <summary>
        /// プロパティに持つ明細Noに対応したBillingDataをフィールドにセットします。
        /// </summary>
        /// <returns></returns>
        public async Task SetTargetBillingData()
        {
            OperationData = await _billingDataService.GetBillingDataAsync(BillingNo);
        }

        #endregion

        #region CommandMethod

        /// <summary>
        /// 金額詳細に表示する金額を算出、描画します。
        /// </summary>
        /// <returns></returns>
        private async Task RenderSplitAmounts()
        {
            // RatioTextから割合を取得する。
            (string left, string right)? separatedRatio = CommonUtil.ExtractSidesByDelimiter(RatioText, ':', false, false);

            if (separatedRatio is null)
            {
                return;
            }

            if (!int.TryParse(separatedRatio.Value.right, out int recipientRatio)) return;

            decimal rate = 0;
            if (recipientRatio != 0) rate = (decimal)recipientRatio / 10;

            // 受領者の割合から算出、TotalAmountから算出した金額を引き請求者の金額も計算
            int recipientAmount = (int)Math.Truncate(TotalAmount * rate);
            int claimAmount = TotalAmount - recipientAmount;

            if (_billingDataDetailPage is null) return;

            // 計算結果を表示
            await _billingDataDetailPage.DisplayAlert(
                "金額詳細"
                ,$"{FromUserName}: {claimAmount:N0}\n{ToUserName}: {recipientAmount:N0}"
                ,Properties.Resources.Ok
                );

        }

        private async Task OnApproveButtonClicked()
        {
            if (GetProcessingStatus()) return;
            isProcessing = true;
            ApproveActionCommand.RaiseCanExecuteChanged();  // 実行処理中はボタンを無効化
            DenyActionCommand.RaiseCanExecuteChanged();

            try
            {
                ErrMsg = string.Empty;
                if (!CheckBillingNoWithObject()) return;

                Debug.WriteLine("CheckClearInApprove");
                
                int rollbackStatusCode = StatusCode;
                switch (StatusCode)
                {
                    case 0:
                        Debug.WriteLine("OnApprove:0");

                        string rollbackDelflg = DeleteFlag;

                        // 取消承認(削除フラグ付与)
                        StatusCode = -1;
                        OperationData!.StatusCode = StatusCode;
                        DeleteFlag = MappingStrResource.DeleteFlagStr;
                        OperationData!.DeleteFlag = MappingStrResource.DeleteFlagStr;

                        if (!await UpdateBillingDataByOperationData(OperationData, rollbackStatusCode, rollbackDelflg)) return;
                        Debug.WriteLine($"更新しました。{nameof(OperationData)}: {OperationData.StatusCode}: {nameof(DeleteFlag)}: {nameof(OperationData.DeleteFlag)}");
                        SetDisplayViewStatus();
                        return;

                    case 1:
                        Debug.WriteLine("OnApprove:1");

                        // 請求書受領者承認
                        StatusCode = 2;
                        OperationData!.StatusCode = StatusCode;

                        //await Task.Delay(10000);  //消す

                        if (!await UpdateBillingDataByOperationData(OperationData, rollbackStatusCode)) return;
                        Debug.WriteLine($"更新しました。{nameof(OperationData)}: {OperationData.StatusCode}");
                        SetDisplayViewStatus();
                        return;

                    case 3:
                        Debug.WriteLine("OnApprove:3");

                        // 支払い済み申請承認
                        StatusCode = 100;  // Complete
                        OperationData!.StatusCode = StatusCode;

                        if (!await UpdateBillingDataByOperationData(OperationData, rollbackStatusCode)) return;
                        Debug.WriteLine($"更新しました。{nameof(OperationData)}: {OperationData.StatusCode}");
                        SetDisplayViewStatus();
                        return;

                    default:
                        throw new StatusCodeInconsistencyException(StatusCode);
                }

            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(OnApproveButtonClicked), null);
                Application.Current!.MainPage = new ErrorPage();
            }
            finally 
            {
                isProcessing = false;
                ProcessMsg = string.Empty;
                ApproveActionCommand.RaiseCanExecuteChanged();
                DenyActionCommand.RaiseCanExecuteChanged();
            }
        }

        private async Task OnDenyButtonClicked()
        {
            if (GetProcessingStatus()) return;
            isProcessing = true;
            ApproveActionCommand.RaiseCanExecuteChanged();  // 実行処理中はボタンを無効化
            DenyActionCommand.RaiseCanExecuteChanged();

            try
            {
                ErrMsg = string.Empty;
                if (!CheckBillingNoWithObject()) return;

                Debug.WriteLine("CheckClearInDeny");

                int rollbackStatusCode = StatusCode;
                switch (StatusCode)
                {
                    case 0:
                        Debug.WriteLine("OnDeny:0");
                        // 取消確認請求者否認
                        StatusCode = 1;
                        OperationData!.StatusCode = StatusCode;

                        if (!await UpdateBillingDataByOperationData(OperationData, rollbackStatusCode)) return;

                        Debug.WriteLine($"更新しました。{nameof(OperationData)}: {OperationData.StatusCode}");
                        SetDisplayViewStatus();
                        return;

                    case 1:
                        Debug.WriteLine("OnDeny:1");

                        // 請求書受領者否認
                        StatusCode = 0;
                        OperationData!.StatusCode = StatusCode;

                        if (!await UpdateBillingDataByOperationData(OperationData, rollbackStatusCode)) return;

                        Debug.WriteLine($"更新しました。{nameof(OperationData)}: {OperationData.StatusCode}");
                        SetDisplayViewStatus();
                        return;
                    case 3:
                        Debug.WriteLine("OnDeny:3");

                        // 支払い済み承認申請拒否
                        StatusCode = 2;
                        OperationData!.StatusCode = StatusCode;

                        if (!await UpdateBillingDataByOperationData(OperationData, rollbackStatusCode)) return;

                        Debug.WriteLine($"更新しました。{nameof(OperationData)}: {OperationData.StatusCode}");
                        SetDisplayViewStatus();
                        return;
                    default:
                        throw new StatusCodeInconsistencyException(StatusCode);
                }
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(OnDenyButtonClicked), null);
                Application.Current!.MainPage = new ErrorPage();
            }
            finally
            {
                isProcessing = false;
                ProcessMsg = string.Empty;
                ApproveActionCommand.RaiseCanExecuteChanged();
                DenyActionCommand.RaiseCanExecuteChanged();
            }
        }

        private async Task OnSendPaymentComplete()
        {
            Debug.WriteLine(nameof(OnSendPaymentComplete));
            if (GetProcessingStatus()) return;
            isProcessing = true;
            SendPaymentCompleteCommand.RaiseCanExecuteChanged();  // 実行処理中はボタンを無効化

            try
            {
                ErrMsg = string.Empty;
                if (!CheckBillingNoWithObject()) return;
                Debug.WriteLine($"CheckClear{nameof(OnSendPaymentComplete)}");

                if (!await _alertService.ShowAlertAsync("確認", "未払いステータスを変更しますか？", "OK", "Cancel")) return;  // ポップアップ上でキャンセルした場合はそのままリターン
                Debug.WriteLine("InStatusEdit");

                if (StatusCode != 2) throw new StatusCodeInconsistencyException(StatusCode);
                int rollbackStatusCode = StatusCode;

                // 支払い済み
                StatusCode = 3;
                OperationData!.StatusCode = StatusCode;

                if (!await UpdateBillingDataByOperationData(OperationData, rollbackStatusCode)) return;
                Debug.WriteLine($"更新しました。{nameof(OperationData)}: {OperationData.StatusCode}");
                SetDisplayViewStatus();
                return;
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(OnSendPaymentComplete), null);
                Application.Current!.MainPage = new ErrorPage();
            }
            finally
            {
                isProcessing = false;
                ProcessMsg = string.Empty;
                SendPaymentCompleteCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Preparation Method

        /// <summary>
        /// ViewModelに対する関連ページの紐づけを行います。
        /// </summary>
        /// <param name="billingDataDetailPage"></param>
        /// <returns></returns>
        public bool SetRelatedPage(BillingDataDetailPage billingDataDetailPage)
        {
            if (billingDataDetailPage is null) return false;

            _billingDataDetailPage = billingDataDetailPage;

            return true;
        }

        #endregion
    }
}

using Expense_Fair_Split.Commons;
using Expense_Fair_Split.DTOs.BillingData;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Models.PickerModels;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.RemoteDB;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.Views.Error;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace Expense_Fair_Split.ViewModels
{
    public class BillingDataInquiryViewModel : Prism.Mvvm.BindableBase
    {
        private readonly UserSessionService _userSessionService;
        private readonly ILogDataService _logDataService;
        private readonly IBillingDataService _billingDataService;
        private readonly IUserService _userService;
        private readonly IAccountDataService _accountDataService;
        private readonly IMDistRatioService _distRatioService;
        private readonly SyncFlagService _syncFlagService;
        private readonly SyncService _syncService;
        private readonly ViewInputStateService _viewInputStateService;
        public DelegateCommand RefreshDataCommand { get; }
        private bool _isProcessing = false;
        private bool _rotateState = false;

        public BillingDataInquiryViewModel() 
        {
            var serviceProvider = App.Services;
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _billingDataService = serviceProvider.GetRequiredService<IBillingDataService>();
            _userService = serviceProvider.GetRequiredService<IUserService>();
            _accountDataService = serviceProvider.GetRequiredService<IAccountDataService>();
            _distRatioService = serviceProvider.GetRequiredService<IMDistRatioService>();
            _syncFlagService = serviceProvider.GetRequiredService<SyncFlagService>();
            _syncService = serviceProvider.GetRequiredService<SyncService>();
            _viewInputStateService = serviceProvider.GetRequiredService<ViewInputStateService>();
            RefreshDataCommand = new DelegateCommand(async () => await OnRefreshDataClicked(), CanExecuteButton);

            Filter1 = SetPropertiesResource.BillingDataFilterProperties.billingDataFilter1ItemObCollection;
            Filter1SelectedIndex = 0;

            Filter2 = SetPropertiesResource.BillingDataFilterProperties.billingDataFilter2ItemObCollection;
            Filter2SelectedIndex = 0;
        }

        #region CanExecuteMethod

        private bool CanExecuteButton()
        {
            return !_isProcessing;  // 処理未実行時のみ押下可能
        }

        #endregion

        #region UI Binding Properties

        public ObservableCollection<BillingDataInquiryGridViewDto> BillingDatas {  get; set; } = new ObservableCollection<BillingDataInquiryGridViewDto>();
        private BillingDataInquiryGridViewDto? _selectedItem = null;
        public BillingDataInquiryGridViewDto SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value, nameof(SelectedItem));
        }

        // Msgエリア領域の可視化判定フラグ
        public bool IsMsgAreaVisible => ShowInfoMsg || ShowErrorMsg || ShowProcessMsg || ShowProcessMsg2;

        #region Info Msg

        private string _infoMsg = string.Empty;
        public string InfoMsg
        {
            get => _infoMsg;
            set
            {
                if (SetProperty(ref _infoMsg, value, nameof(InfoMsg)))
                {
                    ShowInfoMsg = _infoMsg != string.Empty;
                }
            }
        }

        private bool _showInfoMsg = false;
        public bool ShowInfoMsg
        {
            get => _showInfoMsg;
            set
            {
                if (SetProperty(ref _showInfoMsg, value, nameof(ShowInfoMsg)))
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsMsgAreaVisible)));
                }
            }
        }

        #endregion

        #region ErrorMsg

        private string _errorMsg = string.Empty;
        public string ErrorMsg
        {
            get => _errorMsg;
            set
            {
                if (SetProperty(ref _errorMsg, value, nameof(ErrorMsg)))
                {
                    ShowErrorMsg = _errorMsg != string.Empty;
                }
            }
        }

        private bool _showErrorMsg = false;
        public bool ShowErrorMsg
        {
            get => _showErrorMsg;
            set
            {
                if (SetProperty(ref _showErrorMsg, value, nameof(ShowErrorMsg)))
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsMsgAreaVisible)));
                }
            }
        }

        #endregion

        #region ProcessMsg

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
            set
            {
                if (SetProperty(ref _showProcessMsg, value, nameof(ShowProcessMsg)))
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsMsgAreaVisible)));
                }
            }
        }

        private string _processMsg2 = string.Empty;
        public string ProcessMsg2
        {
            get => _processMsg2;
            set
            {
                if (SetProperty(ref _processMsg2, value, nameof(ProcessMsg2)))
                {
                    ShowProcessMsg2 = _processMsg2 != string.Empty;
                }
            }
        }

        private bool _showProcessMsg2 = false;
        public bool ShowProcessMsg2
        {
            get => _showProcessMsg2;
            set
            {
                if (SetProperty(ref _showProcessMsg2, value, nameof(ShowProcessMsg2)))
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsMsgAreaVisible)));
                }
            }
        }

        #endregion

        #region PickerFilterProperties

        // Filter1
        private ObservableCollection<BillingDataFilterItem> _filter1 = new ObservableCollection<BillingDataFilterItem>();
        public ObservableCollection<BillingDataFilterItem> Filter1
        {
            get => _filter1;
            set => SetProperty(ref _filter1, value, nameof(Filter1));
        }
        private BillingDataFilterItem _filter1SelectedItem = null!;
        public BillingDataFilterItem Filter1SelectedItem
        {
            get => _filter1SelectedItem;
            set
            {
                SetProperty(ref _filter1SelectedItem, value, nameof(Filter1SelectedItem));
                Debug.WriteLine(Filter1SelectedItem);
            }
        }
        private int _filter1SelectedIndex = -1;
        public int Filter1SelectedIndex
        {
            get => _filter1SelectedIndex;
            set => SetProperty(ref _filter1SelectedIndex, value, nameof(Filter1SelectedIndex));
        }

        // Filter2
        private ObservableCollection<BillingDataFilterItem> _filter2 = new ObservableCollection<BillingDataFilterItem>();
        public ObservableCollection<BillingDataFilterItem> Filter2
        {
            get => _filter2;
            set => SetProperty(ref _filter2, value, nameof(Filter2));
        }
        private BillingDataFilterItem _filter2SelectedItem = null!;
        public BillingDataFilterItem Filter2SelectedItem
        {
            get => _filter2SelectedItem;
            set
            {
                SetProperty(ref _filter2SelectedItem, value, nameof(Filter2SelectedItem));
                Debug.WriteLine(Filter2SelectedItem);
            }
        }
        private int _filter2SelectedIndex = -1;
        public int Filter2SelectedIndex
        {
            get => _filter2SelectedIndex;
            set => SetProperty(ref _filter2SelectedIndex, value, nameof(Filter2SelectedIndex));
        }

        #endregion

        #region calcProperty

        private int _calcTotalAmount;
        public int CalcTotalAmount
        {
            get => _calcTotalAmount;
            set 
            {
                if (SetProperty(ref _calcTotalAmount, value, nameof(CalcTotalAmount)))
                {
                    IsCalculated = _calcTotalAmount > 0;
                }
            } 
        }

        private int _calcBillingAmount;
        public int CalcBillingAmount
        {
            get => _calcBillingAmount;
            set => SetProperty(ref _calcBillingAmount, value, nameof(CalcBillingAmount));
        }

        private bool _isCalculated;
        public bool IsCalculated
        {
            get => _isCalculated;
            set => SetProperty(ref _isCalculated, value, nameof(IsCalculated));
        }

        #endregion

        #region Motion Property

        private int _rotateTo = 0;
        public int RotateTo
        {
            get => _rotateTo;
            set => SetProperty(ref _rotateTo, value, nameof(RotateTo));
        }

        #endregion

        #endregion

        #region SetProperties Method

        /// <summary>
        /// 明細表データを取得しview用DTOにセットする
        /// </summary>
        /// <param name="filterKey1">
        ///     明細表の請求/受領判別キー
        ///     0: All  1: 請求分データのみ  2: 受領分データのみ
        /// </param>
        /// <param name="filterKey2">
        ///     取引状態別フィルターキー
        ///     0 : All 1: 取引中のみ  2: 完了分のみ  3: 削除済みのみ
        /// </param>
        /// <returns></returns>
        /// <exception cref="NotUserSessionException"></exception>
        /// <exception cref="NotFindItemException"></exception>
        public async Task GetGridViewDto(int filterKey1, int filterKey2)
        {
            if (_userSessionService is null || _userSessionService.UserId == -1) throw new NotUserSessionException();

            BillingDatas.Clear();  // UI再生成のため中身を削除

            /*** メインデータ取得 ***/
            List<BillingData> billingFromUserDataList = new List<BillingData>();  // 請求データ
            List<BillingData> billingToUserDataList = new List<BillingData>();    // 受領分データ

            if (filterKey1 == 1)
            {
                // 請求分データのみ取得
                billingFromUserDataList = (await _billingDataService.GetAllByFromUserCodeFindAsync(_userSessionService.UserId)).ToList();
            }
            else if (filterKey1 == 2)
            {
                // 受領分データのみ取得
                billingToUserDataList = (await _billingDataService.GetAllByToUserCodeFindAsync(_userSessionService.UserId)).ToList();
            }
            else
            {
                billingFromUserDataList = (await _billingDataService.GetAllByFromUserCodeFindAsync(_userSessionService.UserId)).ToList();
                billingToUserDataList = (await _billingDataService.GetAllByToUserCodeFindAsync(_userSessionService.UserId)).ToList();
            }

                

            if (billingFromUserDataList.Count == 0 && billingToUserDataList.Count == 0) return;  // 登録データがない場合はそのままリターン

            // 表示順を（自分から見て）請求されている(To) => 請求している(From)にするためToのListに対しFromのListを紐づけた上で新しいリストを生成する。
            List<BillingData> concatBillingDataList = billingToUserDataList.Concat(billingFromUserDataList).ToList();

            /*** 結合データ取得 && 辞書作成 ***/
            List<User> userList = (await _userService.GetAllUsersAsync()).ToList();
            List<AccountData> accountDataList = (await _accountDataService.GetAllAccountDataAsync()).ToList();
            List<MDistRatio> mDistRatioList = (await _distRatioService.GetAllMDistRatioAsync()).ToList();

            // (Key: user => ID, Value: user => Name)
            Dictionary<int, string> userDict = userList.ToDictionary(user => user.Id, user => user.Name);

            // (Key: account => ID, Value: account => Name)
            Dictionary<int, string> accountDict = accountDataList.ToDictionary(acc => acc.AccId, acc => acc.AccName);

            // (Key: distRatio => (typeCode, code), Value: distRatio => (RatioName, DisplayName))
            Dictionary<(int, int), (string, string)> distRatioDict = mDistRatioList.ToDictionary(ratio => (ratio.RatioTypeCode, ratio.RatioCode),
                                                                                                 ratio => (ratio.RatioName, ratio.RatioDisplayName));

            /*** DTO生成 ***/
            var dtoDataset = concatBillingDataList.Select(billingData =>
            {
                (string from, string to) userName = (userDict.TryGetValue(billingData.FromUserCode, out string? fromUserTargetName) ? fromUserTargetName : string.Empty,
                                                     userDict.TryGetValue(billingData.ToUserCode, out string? toUserTargetName) ? toUserTargetName : string.Empty);

                string accName = accountDict.TryGetValue(billingData.AccountCode, out string? accTargetName) ? accTargetName : string.Empty;

                (string ratioName, string ratioDisplayName) ratioStr = distRatioDict.TryGetValue((billingData.RatioTypeCode, billingData.RatioCode ?? -1),
                                                                                                  out (string, string) result) ? result : ("", "");

                BillingDataInquiryGridViewDto billingDataInquiryGridViewDto = new BillingDataInquiryGridViewDto()
                {
                    CalcTarget = false,
                    OldCalcTarget = false,
                    BillingNo = billingData.BillingNo,
                    BillingDate = billingData.BillingDate,
                    AccountCode = billingData.AccountCode,
                    AccountName = accName,
                    RatioTypeCode = billingData.RatioTypeCode,
                    RatioCode = billingData.RatioCode,
                    RatioName = ratioStr.ratioName,
                    RatioDisplayName = ratioStr.ratioDisplayName,
                    FromUserCode = billingData.FromUserCode,
                    FromUserName = userName.from,
                    ToUserCode = billingData.ToUserCode,
                    ToUserName = userName.to,
                    TotalAmount = billingData.TotalAmount,
                    BillingAmount = billingData.BillingAmount,
                    StatusCode = billingData.StatusCode,
                    Note = billingData.Note,
                    DeleteFlag = billingData.DeleteFlag
                };
                billingDataInquiryGridViewDto.SetDisplayViewStatus();

                return billingDataInquiryGridViewDto;
            }).ToList();

            if (dtoDataset is null || dtoDataset.Count == 0) throw new NotFindItemException($"[{nameof(dtoDataset)}]が取得できませんでした。");

            // filter2に応じた条件でリストを削減する
            if (filterKey2 == 1)
            {
                // 取引中以外を削除
                dtoDataset.RemoveAll(data => data.StatusCode == 100 || data.DeleteFlag == "X");
            }
            else if (filterKey2 == 2)
            {
                // 完了分以外を削除
                dtoDataset.RemoveAll(data => data.StatusCode != 100);
            }
            else if (filterKey2 == 3)
            {
                // 削除済み以外を削除
                dtoDataset.RemoveAll(data => data.DeleteFlag != "X");
            }

            if (dtoDataset.Count == 0) return;  // フィルターにより0件となった場合そのままリターン

            dtoDataset.ForEach(dto =>
            {
                BillingDatas.Add(dto);
            });
        }

        /// <summary>
        /// 更新アイコンをバックグラウンドスレッドにて回転開始します。
        /// </summary>
        private void StartIconRotation()
        {
            _rotateState = true;

            _ = Task.Run(async () =>
            {
                while (_rotateState)
                {
                    RotateTo++;
                    await Task.Delay(5);
                }
            });
        }

        /// <summary>
        /// 更新アイコンの回転を止めます。
        /// </summary>
        private void StopIconRotation()
        {
            _rotateState = false;
            RotateTo = 0;
        }

        #endregion

        #region Validate method

        /// <summary>
        /// ビューモデルのプロセス判定変数を参照し、処理実行中であればエラーメッセージとtrue(実行中)、それ以外はfalseを返します。
        /// </summary>
        /// <returns>
        ///     true: 実行中
        ///     false: 待機中
        /// </returns>
        private bool GetProcessingStatus()
        {
            if (_isProcessing)
            {
                ProcessMsg = $"・{Properties.Resources.isProcessing}";
                return true;
            }
            return false;
        }

        #endregion

        #region CommandMethod

        private async Task OnRefreshDataClicked()
        {
            Debug.WriteLine("In RefreshData");
            ClearMsgArea();

            // 既に実行中かを判定
            if (GetProcessingStatus()) return;
            _isProcessing = true;
            RefreshDataCommand.RaiseCanExecuteChanged();  // 実行処理中はボタンを無効化

            try
            {
                // 同期処理が実行中かを判定（自動同期等で同期処理中の可能性があるため）
                if (_syncFlagService.IsSyncFlagActive())
                {
                    ProcessMsg2 = $"・{Properties.Resources.SyncAutorunInProgressMsg}";
                    Debug.WriteLine("自動実行同期処理中です。");
                    return;
                }

                // アイコンの回転開始
                StartIconRotation();

                bool isSyncSuccess = false;  // 同期処理の成功/失敗
                try
                {
                    (bool isSuccess, string msgTime, string msg) result = await _syncService.RunFullSyncAsync();
                    isSyncSuccess = result.isSuccess;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"手動実行同期処理中にエラー: {ex.Message}");
                    isSyncSuccess = false;
                    await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), ex.Message, _userSessionService.UserId, nameof(OnRefreshDataClicked), null);
                }

                if (!isSyncSuccess)
                {
                    ErrorMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrorMsg);
                    ErrorMsg += $"・手動実行同期処理中にエラーが発生しました";
                    return;
                }

                // 同期処理成功後、最新のデータを使用しグリッドビューを更新する。
                bool isRefreshed = await RefreshGridView((int)EnumResource.RefreshGridViewType.Cache);

                if (!isRefreshed) 
                {
                    Debug.WriteLine("手動実行同期処理中にエラーが発生しました");
                    ErrorMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrorMsg);
                    ErrorMsg += $"・手動実行同期処理中にエラーが発生しました";
                    return;
                }

                Debug.WriteLine("グリッドビュー更新");

                InfoMsg = CommonUtil.InsertNewLineWhenNotBlank(InfoMsg);
                InfoMsg += $"・グリッドビューを更新しました";
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(OnRefreshDataClicked), null);
                Application.Current!.MainPage = new ErrorPage();
            }
            finally
            {
                StopIconRotation();
                _isProcessing = false;
                ProcessMsg = string.Empty;
                RefreshDataCommand.RaiseCanExecuteChanged();
            }   
        }
        #endregion

        #region Public Method

        /// <summary>
        /// 呼び出された時点のフィルター項目をキャッシュに保存します。
        /// </summary>
        /// <returns></returns>
        public bool SaveCurrentFilterToCache()
        {
            if (Filter1SelectedIndex != -1 && Filter2SelectedIndex != -1)
            {
                if (Filter1SelectedItem is not null && Filter2SelectedItem is not null)
                {
                    int filterKey1 = Filter1SelectedItem.Id;
                    int filterKey2 = Filter2SelectedItem.Id;

                    if (_viewInputStateService is not null)
                    {
                        BillingDataInquiryInputState filterCache = _viewInputStateService.BillingDataInquiry;

                        filterCache.Filter1Index = filterKey1;
                        filterCache.Filter2Index = filterKey2;

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 明細の計算プロパティを初期化します。
        /// </summary>
        public void ClearCalculationState()
        {
            CalcTotalAmount = 0;
            CalcBillingAmount = 0;
        }

        /// <summary>
        /// 画面のグリッドビュー表示データを最新化して更新します。
        /// </summary>
        /// <param name="refreshType">
        ///     0: 通常パターン（起動時点のUI画面上に表示フィルター情報を使用）
        ///     1: キャッシュからフィルター情報を取得
        /// </param>
        /// <returns>
        ///     true: 成功
        ///     false: 失敗（例外発生時のみ失敗扱いとする）
        /// </returns>
        public async Task<bool> RefreshGridView(int refreshType) 
        {
            bool isSuccess = true;  // 処理の成功/失敗（例外発生時のみ失敗扱いとする）
            bool shouldFallbackToNormal = false;  // 再帰するかの判定フラグ

            try
            {
                // 計算金額リセット
                ClearCalculationState();

                switch (refreshType)
                {
                    case (int)EnumResource.RefreshGridViewType.Normal:

                        if (Filter1SelectedIndex != -1 && Filter2SelectedIndex != -1)
                        {
                            if (Filter1SelectedItem is not null && Filter2SelectedItem is not null)
                            {
                                int filterKey1 = Filter1SelectedItem.Id;
                                int filterKey2 = Filter2SelectedItem.Id;
                                await GetGridViewDto(filterKey1, filterKey2);
                            }
                            else
                            {
                                Debug.WriteLine("生成なし: Item=null");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("生成なし: インデックス番号-1");
                        }

                        break;
                    case (int)EnumResource.RefreshGridViewType.Cache:

                        if (_viewInputStateService is not null)
                        {
                            BillingDataInquiryInputState filterCache = _viewInputStateService.BillingDataInquiry;

                            if (filterCache.Filter1Index is not null || filterCache.Filter2Index is not null)
                            {
                                int filter1 = filterCache.Filter1Index ?? 0;
                                int filter2 = filterCache.Filter2Index ?? 0;

                                await GetGridViewDto(filter1, filter2);

                                Filter1SelectedIndex = filter1;
                                Filter2SelectedIndex = filter2;
                            }
                            else
                            {
                                shouldFallbackToNormal = true;
                            }
                        }
                        else
                        {
                            shouldFallbackToNormal = true;
                        }
                        break;
                    default:

                        await _logDataService.InsertLog(EnumResource.LogLevel.DEBUG.ToString(), "引数に対象範囲外のパラメータが渡されています。", null, nameof(RefreshGridView), null);
                        shouldFallbackToNormal = true;
                        break;
                }

                // 通常パターン以外で問題があった場合は、通常パターンにフォールバック
                if (shouldFallbackToNormal)
                {
                    isSuccess = await RefreshGridView((int)EnumResource.RefreshGridViewType.Normal);
                }
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), $"{Properties.Resources.RefreshGridViewFailed} + {ex.Message}", _userSessionService.UserId, nameof(RefreshGridView), null);
                isSuccess = false;  // 例外発生時のみ失敗扱いとする。
                return isSuccess;  
            }

            return isSuccess;            
        }

        #endregion

        #region Private Method

        /// <summary>
        /// メッセージを一部を除き削除します。
        /// </summary>
        private void ClearMsgArea()
        {
            ErrorMsg = string.Empty;
            ProcessMsg2 = string.Empty;
            InfoMsg = string.Empty;
        }

        #endregion
    }
}

using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Models.PickerModels;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.Views;
using Expense_Fair_Split.Views.Error;
using Expense_Fair_Split.Views.ImageRecognition;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Expense_Fair_Split.ViewModels
{
    public class BillingDataEntryViewModel : Prism.Mvvm.BindableBase
    {
        public DelegateCommand StackAmountCommand { get; }

        private readonly IUserService _userService;
        private readonly IAccountDataService _accountDataService;
        private readonly IMDistRatioService _distRatioService;
        private readonly ILogDataService _logDataService;
        private readonly UserSessionService _userSessionService;
        private readonly BillingDataEntryPage _billingDataEntryPage;

        public BillingDataEntryViewModel(BillingDataEntryPage billingDataEntryPage)
        {
            var serviceProvider = App.Services;
            _userService = serviceProvider.GetRequiredService<IUserService>();
            _accountDataService = serviceProvider.GetRequiredService<IAccountDataService>();
            _distRatioService = serviceProvider.GetRequiredService<IMDistRatioService>();
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _billingDataEntryPage = billingDataEntryPage;

            FromName = _userSessionService.UserName;
            StackAmountCommand = new DelegateCommand(() => IncludeStackAmountInTotalAmount());
        }

        #region UI Binding Properties
        public string FromName { get; }

        #region Picker Properties

        private ObservableCollection<ToPickerItem> _toPick = new ObservableCollection<ToPickerItem>();
        public ObservableCollection<ToPickerItem> ToPick
        {
            get => _toPick;
            set => SetProperty(ref _toPick, value, nameof(ToPick));
        }
        private ToPickerItem _toPickSelectedItem = null!;
        public ToPickerItem ToPickSelectedItem
        {
            get => _toPickSelectedItem;
            set => SetProperty(ref _toPickSelectedItem, value, nameof(ToPickSelectedItem));
        }
        private int _toPickSelectedIndex = 0;
        public int ToPickSelectedIndex
        {
            get => _toPickSelectedIndex;
            set => SetProperty(ref _toPickSelectedIndex, value, nameof(ToPickSelectedIndex));
        }

        private ObservableCollection<AccPickerItem> _accPick = new ObservableCollection<AccPickerItem>();
        public ObservableCollection<AccPickerItem> AccPick
        {
            get => _accPick;
            set => SetProperty(ref _accPick, value, nameof(AccPick));
        }
        private AccPickerItem _accPickSelectedItem = null!;
        public AccPickerItem AccPickSelectedItem
        {
            get => _accPickSelectedItem;
            set => SetProperty(ref _accPickSelectedItem, value, nameof(AccPickSelectedItem));
        }
        private int _accPickSelectedIndex = 0;
        public int AccPickSelectedIndex
        {
            get => _accPickSelectedIndex;
            set => SetProperty(ref _accPickSelectedIndex, value, nameof(AccPickSelectedIndex));
        }

        private ObservableCollection<RatioPickerItem> _ratioPick = new ObservableCollection<RatioPickerItem>();
        public ObservableCollection<RatioPickerItem> RatioPick
        {
            get => _ratioPick;
            set => SetProperty(ref _ratioPick, value, nameof(RatioPick));
        }
        private RatioPickerItem _ratioPickSelectedItem = null!;
        public RatioPickerItem RatioPickSelectedItem
        {
            get => _ratioPickSelectedItem;
            set => SetProperty(ref _ratioPickSelectedItem, value, nameof(RatioPickSelectedItem));
        }
        private int _ratioPickSelectedIndex = 0;
        public int RatioPickSelectedIndex
        {
            get => _ratioPickSelectedIndex;
            set => SetProperty(ref _ratioPickSelectedIndex, value, nameof(RatioPickSelectedIndex));
        }

        private List<CalcInputModePickerItem> _inputModePick = new List<CalcInputModePickerItem>()
        {
            new() { CalcInputMode = 1, CalcInputModeDisplayName = "一括" },
            new() { CalcInputMode = 2, CalcInputModeDisplayName = "個別" },
            new() { CalcInputMode = 3, CalcInputModeDisplayName = "画像認識" }
        };
        public List<CalcInputModePickerItem> InputModePick
        {
            get => _inputModePick;
            set => SetProperty(ref _inputModePick, value, nameof(InputModePick));
        }
        private CalcInputModePickerItem _inputModePickSelectedItem = null!;
        public CalcInputModePickerItem InputModePickSelectedItem
        {
            get => _inputModePickSelectedItem;
            set
            {
                if (SetProperty(ref _inputModePickSelectedItem, value, nameof(InputModePickSelectedItem)))
                {
                    StackCalcAmount = null;

                    // 一括の場合、追加金額入力欄は非表示にする。
                    if (InputModePickSelectedItem.CalcInputMode == 1)
                    {
                        TotalAmount = null;
                        StackCalcIsVisible = false;
                        TotalAmountIsEnable = true;
                    }
                    else if (InputModePickSelectedItem.CalcInputMode == 2)
                    {
                        TotalAmount = 0;
                        StackCalcIsVisible = true;
                        TotalAmountIsEnable = false;
                    }
                    else if (InputModePickSelectedItem.CalcInputMode == 3)
                    {
                        TotalAmount = 0;
                        StackCalcIsVisible = true;
                        TotalAmountIsEnable = false;

                        OnBeforeImageRecognitionPage();
                    }
                }
            }
        }
        private int _inputModePickSelectedIndex = 0;
        public int InputModePickSelectedIndex
        {
            get => _inputModePickSelectedIndex;
            set => SetProperty(ref _inputModePickSelectedIndex, value, nameof(InputModePickSelectedIndex));
        }

        #endregion

        private int? _stackCalcAmount = null;
        public int? StackCalcAmount
        {
            get => _stackCalcAmount;
            set => SetProperty(ref _stackCalcAmount, value, nameof(StackCalcAmount));
        }

        private bool _stackCalcIsVisible = true;
        public bool StackCalcIsVisible
        {
            get => _stackCalcIsVisible;
            set => SetProperty(ref _stackCalcIsVisible, value, nameof(StackCalcIsVisible));
        }

        private int? _totalAmount = null;
        public int? TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value, nameof(TotalAmount));
        }

        private bool _totalAmountIsEnable = true;
        public bool TotalAmountIsEnable
        {
            get => _totalAmountIsEnable;
            set => SetProperty(ref _totalAmountIsEnable, value, nameof(TotalAmountIsEnable));
        }

        public int AmountBilled { get; set; } = 0;

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
        #endregion

        #region SetProperties method
        /// <summary>
        /// プロパティに持つピッカーの初期設定を行います。
        /// </summary>
        /// <returns></returns>
        public async Task PickersInitAsync()
        {
            try
            {
                Task[] tasks = [SetToPickPropertyAsync(), SetAccPickPropertyAsync(), SetRatioPickProperty()];

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(PickersInitAsync), null);
                Application.Current!.MainPage = new ErrorPage();
            }
        }

        private async Task SetToPickPropertyAsync()
        {
            // データの取得に時間がかかった時のため追加しておく
            this.ToPick.Add(new ToPickerItem { Id = -1, Name = "データ取得中..."});
            ToPickSelectedItem = this.ToPick[0];


            User? OwnUser = await _userService.GetUserAsync(_userSessionService.UserId);
            if (OwnUser is null) 
            {
                throw new NotUserSessionException();
            }

            List<User> userList = (await _userService.GetAllUsersAsync()).ToList();
            if (userList is null)
            {
                throw new NotFindItemException($"{nameof(User)}を取得できませんでした。");
            }
            else 
            {
                if (!userList.Remove(OwnUser))
                {
                    throw new NotFindItemException($"削除対象の{nameof(User)}が見つかりませんでした。");
                }
                
                // 一時的に追加しておいたものを削除しデータを追加する
                this.ToPick.Clear();
                if (userList.Count == 0)
                {
                    this.ToPick.Add(new ToPickerItem { Id = -1, Name = $"取得できませんでした。{Properties.Resources.SystemAlert}" });
                }
                else 
                {
                    this.ToPick.Add(new ToPickerItem { Id = -1, Name = "請求相手を選択してください" });
                    foreach (User user in userList)
                    {
                        this.ToPick.Add(new ToPickerItem { Id = user.Id, Name = user.Name});
                    }
                }
                ToPickSelectedItem = this.ToPick[0];
            }
        }

        private async Task SetAccPickPropertyAsync()
        {
            // データの取得に時間がかかった時のため追加しておく
            this.AccPick.Add(new AccPickerItem { Id = -1, AccName = "データ取得中..." });
            AccPickSelectedItem = this.AccPick[0];

            List<AccountData> accountDataList = (await _accountDataService.GetAllAccountDataAsync()).ToList();

            // 取得データから削除フラグTrueのものを除く
            accountDataList.RemoveAll(data => data.DelFlg == true);

            /*** ピッカーへセット ***/
            this.AccPick.Clear();
            if (accountDataList is null || accountDataList.Count == 0)
            {
                this.AccPick.Add(new AccPickerItem { Id = -1, AccName = $"取得できませんでした。{Properties.Resources.CheckTheMaster}" });
            }
            else
            {
                this.AccPick.Add(new AccPickerItem { Id = -1, AccName = "勘定を選択してください" });
                foreach (AccountData accountData in accountDataList) 
                {
                    this.AccPick.Add(new AccPickerItem { Id = accountData.AccId, AccName = accountData.AccName});
                }
            }
            AccPickSelectedItem = this.AccPick[0];
        }

        private async Task SetRatioPickProperty()
        {
            // データの取得に時間がかかった時のため追加しておく
            this.RatioPick.Add(new RatioPickerItem { RatioCode = -1, RatioName = "データ取得中...", RatioDisplayName = "データ取得中..." });
            RatioPickSelectedItem = this.RatioPick[0];

            // ここで取得リスト
            const int TYPECODE = 1;  // 現在はTypeCode1しかないので定数で渡す
            List<MDistRatio> distRatioList = [.. (await _distRatioService.GetAllByRatioTypeCodeFindAsync(TYPECODE)).OrderBy(x => x.RatioCode)];

            this.RatioPick.Clear();
            if (distRatioList.Count == 0)
            {
                this.RatioPick.Add(new RatioPickerItem { RatioCode = -1, RatioName = $"取得できませんでした。{Properties.Resources.SystemAlert}", RatioDisplayName = $"取得できませんでした。{Properties.Resources.SystemAlert}" });
            }
            else 
            {
                this.RatioPick.Add(new RatioPickerItem { RatioCode = -1, RatioName = "請求割合を選択してください", RatioDisplayName = "請求割合を選択してください" });
                foreach (MDistRatio distRatio in distRatioList)
                {
                    this.RatioPick.Add(new RatioPickerItem { RatioCode = distRatio.RatioCode, RatioName = distRatio.RatioName, RatioDisplayName = distRatio.RatioDisplayName });
                }
            }
            RatioPickSelectedItem = this.RatioPick[0];
            Debug.WriteLine("終わったよ");  // 消す
        }
        #endregion

        #region Logic Method
        /// <summary>
        /// 総金額を引数にて渡された係数コード毎に定められた比率で除算する
        /// </summary>
        /// <param name="totalAmount">総金額</param>
        /// <param name="successFlg">処理成功/失敗</param>
        /// <param name="coefficientCode">係数コード</param>
        /// <returns>除算後の金額</returns>
        public int DivideTotalAmount(int? totalAmount, ref bool successFlg, int coefficientCode)
        {
            int divideAmount = 0;
            if (totalAmount is null || totalAmount <= 0) 
            {
                successFlg = false;
                return divideAmount;
            }

            switch (coefficientCode)
            {
                case 1:
                    divideAmount = 0;
                    break;
                case 2:
                    divideAmount = (int)Math.Truncate(totalAmount.Value * 0.1);
                    break;
                case 3:
                    divideAmount = (int)Math.Truncate(totalAmount.Value * 0.2);
                    break;
                case 4:
                    divideAmount = (int)Math.Truncate(totalAmount.Value * 0.3);
                    break;
                case 5:
                    divideAmount = (int)Math.Truncate(totalAmount.Value * 0.4);
                    break;
                case 6:
                    divideAmount = (int)Math.Truncate(totalAmount.Value * 0.5);
                    break;
                case 7:
                    divideAmount = (int)Math.Truncate(totalAmount.Value * 0.6);
                    break;
                case 8:
                    divideAmount = (int)Math.Truncate(totalAmount.Value * 0.7);
                    break;
                case 9:
                    divideAmount = (int)Math.Truncate(totalAmount.Value * 0.8);
                    break;
                case 10:
                    divideAmount = (int)Math.Truncate(totalAmount.Value * 0.9);
                    break;
                case 11:
                    divideAmount = totalAmount.Value;
                    break;
                default:
                    successFlg = false;
                    Console.WriteLine($"{nameof(DivideTotalAmount)}に渡されたコード値が不正です。");
                    return divideAmount;
            }
            successFlg = true;
            return divideAmount;
        }
        #endregion

        #region Command Method

        /// <summary>
        /// StackAmountの金額をTotalAmountに含める
        /// </summary>
        private void IncludeStackAmountInTotalAmount()
        {
            if (StackCalcAmount is null) return;

            TotalAmount += StackCalcAmount;

            if (TotalAmount < 0)
            {
                TotalAmount = 0;
            }

            StackCalcAmount = null;
        }

        #endregion

        private async void OnBeforeImageRecognitionPage()
        {
            await _billingDataEntryPage.Navigation.PushModalAsync(new BeforeImageRecognitionPage(this));
        }
    }
}

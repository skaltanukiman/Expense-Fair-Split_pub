using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Data;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.ViewModels
{
    public partial class ArchivePreparationViewModel : Prism.Mvvm.BindableBase
    {
        private readonly UserSessionService _userSessionService;
        private readonly ILogDataService _logDataService;
        private readonly IBillingDataService _billingDataService;

        private bool _isOutputProcessing = false;

        public enum ProcessSelector
        {
            Output = 0
        }

        public List<BillingData>? CachedBillingDataList { get; private set; } = null;

        public ArchivePreparationViewModel()
        {
            var serviceProvider = App.Services;
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _billingDataService = serviceProvider.GetRequiredService<IBillingDataService>();
        }

        #region UI Binding Properties

        // 年のプルダウン
        private List<string> _targetYearList = new List<string>();
        public List<string> TargetYearList
        {
            get => _targetYearList;
            set => SetProperty(ref _targetYearList, value, nameof(TargetYearList));
        }
        private string _yearPickSelectedItem = null!;
        public string YearPickSelectedItem
        {
            get => _yearPickSelectedItem;
            set
            {
                if (SetProperty(ref _yearPickSelectedItem, value, nameof(YearPickSelectedItem)))
                {
                    if (YearPickSelectedItem is not null)
                    {
                        if (_yearToTargetMonthDic.TryGetValue(YearPickSelectedItem, out List<string>? monthList))
                        {
                            MonthPickSelectedIndex = 0;  // プルダウンを変更した際に範囲外が選択されると例外が発生するので0を指定
                            AvailableMonths = monthList;

                            // 月プルダウンの初期値設定
                            if (AvailableMonths.FirstOrDefault() is not null) MonthPickSelectedItem = AvailableMonths.FirstOrDefault()!;
                        }
                        else
                        {
                            AvailableMonths = new List<string>();
                        }
                    }
                }
            }
        }
        private int _yearPickSelectedIndex = 0;
        public int YearPickSelectedIndex
        {
            get => _yearPickSelectedIndex;
            set => SetProperty(ref _yearPickSelectedIndex, value, nameof(YearPickSelectedIndex));
        }

        // 年毎の存在月を保持する辞書
        private Dictionary<string, List<string>> _yearToTargetMonthDic = new Dictionary<string, List<string>>();

        // 月のプルダウン
        private List<string> _availableMonths = new List<string>();
        public List<string> AvailableMonths
        {
            get => _availableMonths;
            set => SetProperty(ref _availableMonths, value, nameof(AvailableMonths));
        }
        private string _monthPickSelectedItem = null!;
        public string MonthPickSelectedItem
        {
            get => _monthPickSelectedItem;
            set => SetProperty(ref _monthPickSelectedItem, value, nameof(MonthPickSelectedItem));
        }
        private int _monthPickSelectedIndex = 0;
        public int MonthPickSelectedIndex
        {
            get => _monthPickSelectedIndex;
            set => SetProperty(ref _monthPickSelectedIndex, value, nameof(MonthPickSelectedIndex));
        }

        // 削除済みデータを含むか
        private bool _includingDeletedData = false;
        public bool IncludingDeletedData
        {
            get => _includingDeletedData;
            set => SetProperty(ref _includingDeletedData, value, nameof(IncludingDeletedData));
        }

        // 「出力ボタン」の活性/非活性
        private bool _outputButtonEnabled = true;
        public bool OutputButtonEnabled
        {
            get => _outputButtonEnabled;
            set => SetProperty(ref _outputButtonEnabled, value, nameof(OutputButtonEnabled));
        }

        // 「読み込み」オーバーレイの表示
        private bool _showLoadingOverlay = false;
        public bool ShowLoadingOverlay
        {
            get => _showLoadingOverlay;
            set => SetProperty(ref _showLoadingOverlay, value, nameof(ShowLoadingOverlay));
        }

        #endregion

        #region Initialize Method

        /// <summary>
        /// 画面に表示するプルダウンの初期設定
        /// </summary>
        /// <returns></returns>
        public async Task InitPickerListAsync()
        {
            int userId = _userSessionService.UserId;

            if (userId == -1)
            {
                // ユーザーセッション不正時の処理
                throw new NotUserSessionException();
            }

            /* DBの明細データから自身に関係するデータを全て取得する */
            // 同時に検索
            Task<IEnumerable<BillingData>> fromTask = _billingDataService.GetAllByFromUserCodeFindAsync(userId);
            Task<IEnumerable<BillingData>> toTask = _billingDataService.GetAllByToUserCodeFindAsync(userId);

            // 両方終わるまで待機
            await Task.WhenAll(fromTask, toTask);

            // 結果を取得
            List<BillingData> fromResult = (await fromTask).ToList();
            List<BillingData> toResult = (await toTask).ToList();

            if (fromResult.Count == 0 && toResult.Count == 0)
            {
                OutputButtonEnabled = false;
                return;
            }

            List<BillingData> billingDataList = fromResult.Concat(toResult).ToList();

            // 次画面で同じデータを使用するためコピーする。（ディープコピーではないため取り扱い注意）
            CachedBillingDataList = billingDataList;
            
            List<string> yearList = billingDataList
                .Select(data => data.BillingDate.Year.ToString())
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            TargetYearList = yearList;

            // 対象年毎の存在月を生成する
            foreach (string targetYear in yearList)
            {
                // 処理対象年データの存在月を保持するリスト
                List<string> targetYearToMonth = new List<string>();

                targetYearToMonth = billingDataList
                    .Where(data => data.BillingDate.Year.ToString() == targetYear)
                    .Select(data => data.BillingDate.Month)
                    .Distinct()
                    .OrderBy(m => m)
                    .Select(m => m.ToString())
                    .ToList();

                if (targetYearToMonth.Count == 0) targetYearToMonth.Add("取得できませんでした。");

                // １桁月のプレフィックスに0を付与
                for (int i = 0; i < targetYearToMonth.Count; i++)
                {
                    string convertStr = CommonUtil.FormatMonthWithZero(int.Parse(targetYearToMonth[i]));

                    if (convertStr != "変換処理に失敗しました。") targetYearToMonth[i] = convertStr;
                }

                // Key:年 Value:年に対応する存在月を保持
                _yearToTargetMonthDic.Add(targetYear, targetYearToMonth);
            }

            // 年のプルダウンの初期値設定
            if (TargetYearList.FirstOrDefault() is not null) YearPickSelectedItem = TargetYearList.FirstOrDefault()!;
        }

        #endregion

        #region Process Status Method

        public void StartProcessing(ProcessSelector process)
        {
            switch (process)
            {
                case ProcessSelector.Output:
                    OutputButtonEnabled = false;
                    _isOutputProcessing = true;
                    ShowLoadingOverlay = true;
                    break;
            }
        }

        public void EndProcessing(ProcessSelector process)
        {
            switch (process)
            {
                case ProcessSelector.Output:
                    _isOutputProcessing = false;
                    OutputButtonEnabled = true;
                    ShowLoadingOverlay = false;
                    break;
            }
        }

        /// <summary>
        /// 呼び出し時点の処理状態を確認します
        /// </summary>
        /// <param name="process">確認対象プロセス</param>
        /// <returns></returns>
        public bool? CheckProcessStatus(ProcessSelector process)
        {
            switch (process)
            {
                case ProcessSelector.Output:
                    return _isOutputProcessing;
                default:
                    return null;
            }
        }

        #endregion
    }
}

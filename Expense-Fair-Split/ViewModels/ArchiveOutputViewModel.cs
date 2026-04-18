using Expense_Fair_Split.DTOs.Archive;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.ViewModels
{
    public partial class ArchiveOutputViewModel : Prism.Mvvm.BindableBase
    {
        private readonly UserSessionService _userSessionService;
        private readonly ILogDataService _logDataService;
        private readonly IBillingDataService _billingDataService;
        private readonly IUserService _userService;
        private readonly IAccountDataService _accountDataService;
        private readonly IMDistRatioService _distRatioService;

        private readonly List<BillingData> _useBillingDataList;  // 自身に関係する明細データが全て入ったリスト

        public FindSearchKeys SearchKeys { get; } = new FindSearchKeys();

        public ArchiveOutputViewModel(List<BillingData> billingDatas)
        {
            var serviceProvider = App.Services;
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _billingDataService = serviceProvider.GetRequiredService<IBillingDataService>();
            _userService = serviceProvider.GetRequiredService<IUserService>();
            _accountDataService = serviceProvider.GetRequiredService<IAccountDataService>();
            _distRatioService = serviceProvider.GetRequiredService<IMDistRatioService>();

            _useBillingDataList = billingDatas;
        }

        #region Inner Class

        public class FindSearchKeys
        {
            public string SearchYear { get; private set; } = string.Empty;
            public string SearchMonth { get; private set; } = string.Empty;
            public bool IncludingDeletedData { get; private set; } = false;
            public int RatioType { get; } = 1;  // 現状1しかないため固定とする

            public void SetFindKeys(string year, string month, bool deletedSearch)
            {
                this.SearchYear = year;
                this.SearchMonth = month;
                this.IncludingDeletedData = deletedSearch;
            }

            private bool PropertiesHasValue()
            {
                if (this.SearchYear == string.Empty || this.SearchMonth == string.Empty) return false;
                return true;
            }
        }

        #endregion

        #region UI Binding Properties

        private List<ArchiveOutputViewDto> _archiveOutputList = new List<ArchiveOutputViewDto>();
        public List<ArchiveOutputViewDto> ArchiveOutputList
        {
            get => _archiveOutputList;
            set => SetProperty(ref _archiveOutputList, value, nameof(ArchiveOutputList));
        }

        #endregion

        #region UI Visible Properties

        /*** 出力対象が存在するか ***/
        private bool _noCollection = false;
        public bool NoCollection
        {
            get => _noCollection;
            set => SetProperty(ref _noCollection, value, nameof(NoCollection));
        }

        /*** 有効データが存在するか ***/
        private bool _hasValidData = false;
        public bool HasValidData
        {
            get => _hasValidData;
            set => SetProperty(ref _hasValidData, value, nameof(HasValidData));
        }

        /*** 削除済みデータが存在するか ***/
        private bool _hasDeletedData = false;
        public bool HasDeletedData
        {
            get => _hasDeletedData;
            set => SetProperty(ref _hasDeletedData, value, nameof(HasDeletedData));
        }

        #endregion

        /// <summary>
        /// 画面表示データをViewModelプロパティのDTOへバインドする
        /// </summary>
        /// <returns></returns>
        public async Task CreateViewDto()
        {
            const string DELETE_FLG = "X";
            int searchYear = int.Parse(this.SearchKeys.SearchYear);
            int searchMonth = int.Parse(this.SearchKeys.SearchMonth);

            List<BillingData> extractedBilllingList = new List<BillingData>();

            if (this.SearchKeys.IncludingDeletedData)
            {
                // 削除済みデータを含む
                extractedBilllingList = _useBillingDataList.Where(data => data.BillingDate.Year == searchYear &&
                                                                          data.BillingDate.Month == searchMonth &&
                                                                          data.RatioTypeCode == this.SearchKeys.RatioType)
                                                           .ToList();
            }
            else
            {
                // 削除済みデータを含まない
                extractedBilllingList = _useBillingDataList.Where(data => data.BillingDate.Year == searchYear &&
                                                                          data.BillingDate.Month == searchMonth &&
                                                                          data.RatioTypeCode == this.SearchKeys.RatioType &&
                                                                          data.DeleteFlag != DELETE_FLG)
                                                           .ToList();
            }

            if (extractedBilllingList.Count == 0)
            {
                NoCollection = true;
            }

            /*** 結合データ取得 && 辞書作成 ***/
            List<User> userList = (await _userService.GetAllUsersAsync()).ToList();
            List<AccountData> accountDataList = (await _accountDataService.GetAllAccountDataAsync()).ToList();
            List<MDistRatio> mDistRatioList = (await _distRatioService.GetAllMDistRatioAsync()).ToList();

            /*** DTO生成 ***/
            ArchiveOutputList = extractedBilllingList.Select(data =>
            {
                string year = data.BillingDate.Year.ToString();
                string month = data.BillingDate.Month.ToString("D2");
                string day = data.BillingDate.Day.ToString("D2");
                string date = $"{year}/{month}/{day}";

                ArchiveOutputViewDto dto = new ArchiveOutputViewDto
                {
                    BillingDate = date,
                    FromUser = userList.FirstOrDefault(user => user.Id == data.FromUserCode)?.Name ?? "",
                    ToUser = userList.FirstOrDefault(user => user.Id == data.ToUserCode)?.Name ?? "",
                    AccountName = accountDataList.FirstOrDefault(acc => acc.AccId == data.AccountCode)?.AccName ?? "",
                    Ratio = mDistRatioList.FirstOrDefault(m => m.RatioCode == data.RatioCode && m.RatioTypeCode == data.RatioTypeCode)?.RatioDisplayName ?? "",
                    TotalAmount = data.TotalAmount,
                    Note = data.Note,
                    DelFlg = data.DeleteFlag
                };
                return dto;
            }).ToList();

            if (ArchiveOutputList.Count == 0)
            {
                NoCollection = true;
            }

            HasValidData = ArchiveOutputList.Any(data => data.DelFlg != DELETE_FLG);
            HasDeletedData = ArchiveOutputList.Any(data => data.DelFlg == DELETE_FLG);
        }
    }
}

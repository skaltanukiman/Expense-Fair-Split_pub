using Expense_Fair_Split.Commons;
using Expense_Fair_Split.DTOs.AccountData;
using Expense_Fair_Split.DTOs.BillingData;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Models.PickerModels;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using Syncfusion.Maui.Data;
using System.Diagnostics;
using System.Reflection;

namespace Expense_Fair_Split.Views;

public partial class BillingDataInquiryPage : ContentPage
{
    private readonly UserSessionService _userSessionService;
    private readonly ViewInputStateService _viewInputStateService;
    private readonly ILogDataService _logDataService;
    private string? _cachedPopup = null;

    public BillingDataInquiryPage()
	{
		InitializeComponent();
        var serviceProvider = App.Services;
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
        _viewInputStateService = serviceProvider.GetRequiredService<ViewInputStateService>();

        // Win,Androidで戻るボタンの挙動が違うのでLoadedの内容をOnNavigatedToへ移行
    }
	BillingDataInquiryViewModel? _vm;

    #region Loaded Event

    /// <summary>
    /// BillingDataInquiryPageに遷移した際のイベント処理
    /// </summary>
    /// <param name="args"></param>
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // 戻るボタン押下時、キャッシュ処理の違いの関係により挙動が違うので、挙動を統一するためにOnNavigatedTo内でビューモデルを読み込む&バインディングする
        try
        {
            _vm = new BillingDataInquiryViewModel();
            this.BindingContext = _vm;

            // 20250608mod 初期表示時以外はキャッシュから明細詳細画面遷移時の最終入力値を取得し、取得条件でフィルターを掛けUIを再生成する。
            await _vm.RefreshGridView((int)EnumResource.RefreshGridViewType.Cache);

        }
        catch (Exception ex)
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(OnNavigatedTo), null);
            Application.Current!.MainPage = new ErrorPage();
        }
    }

    #endregion

    #region Event Method

    /// <summary>
    /// 明細表の項目をタップした際に発火するイベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnBillingDataDetail(object sender, SelectionChangedEventArgs e)
	{
        try
        {
            if (_vm is null) throw new ViewModelNotFoundException(nameof(BillingDataInquiryViewModel));

            CollectionView? collectionView = sender as CollectionView;
            BillingDataInquiryGridViewDto? item = e.CurrentSelection.FirstOrDefault() as BillingDataInquiryGridViewDto;
            if (item is null || collectionView is null)
            {
                return;
            }

            // 詳細ページへ遷移処理&CollectionViewをnullに（画面遷移先から戻った際に、同じ項目を再度タップしても反応しなくなるため）
            await this.Navigation.PushAsync(new BillingDataDetailPage(new BillingDataDetailViewModel
            {
                BillingNo = item.BillingNo,
                BillingDate = item.BillingDate,
                AccountName = item.AccountName,
                FromUserCode = item.FromUserCode,
                FromUserName = item.FromUserName,
                ToUserCode = item.ToUserCode,
                ToUserName = item.ToUserName,
                RatioText = item.RatioDisplayName,
                TotalAmount = item.TotalAmount,
                AmountBilled = item.BillingAmount,
                StatusCode = item.StatusCode,
                Note = item.Note,
                DeleteFlag = item.DeleteFlag
            }));
            collectionView.SelectedItem = null;

            // 画面遷移時、フィルターの最終入力値をキャッシュに保存
            bool saveSuccess = _vm.SaveCurrentFilterToCache();

            if (!saveSuccess) await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), "フィルター情報をキャッシュ保存時に問題が発生しました。", _userSessionService.UserId, nameof(OnBillingDataDetail), null);

        }
        catch (Exception ex) 
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(OnBillingDataDetail), null);
            Application.Current!.MainPage = new ErrorPage();
        }
    }

    /// <summary>
    /// フィルターピッカー変更時イベント（明細表のUI出力データを切り替える）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ViewModelNotFoundException"></exception>
    /// <exception cref="NotFindItemException"></exception>
    private async void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
		try
		{
			if (_vm is null) throw new ViewModelNotFoundException(nameof(BillingDataInquiryViewModel));
            if (sender as Picker is not Picker picker) throw new NotFindItemException($"{nameof(Picker_SelectedIndexChanged)}に渡されたオブジェクトが[{nameof(Picker)}]ではありません。");

            int selectedVal = picker.SelectedIndex;
            if (selectedVal == -1) return;  // -1の場合そのままリターン

            // GridView生成メソッド呼び出し
            await _vm.RefreshGridView((int)EnumResource.RefreshGridViewType.Normal);

        }
        catch (Exception ex)
		{
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(Picker_SelectedIndexChanged), null);
            Application.Current!.MainPage = new ErrorPage();
        }    
    }

    /// <summary>
    /// 画面の!アイコンを押下した際の処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CircleExclamation_Clicked(object sender, EventArgs e)
    {
        string content = string.Empty;

        try
        {
            if (string.IsNullOrEmpty(_cachedPopup))
            {
                string getFileName = "CircleExclamation_Popup";
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
            await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), $"ストリーム読み取り処理の中で問題が発生しました。({ex.Message})", _userSessionService.UserId, nameof(CircleExclamation_Clicked), null);            
        }

        await DisplayAlert("Description", content, "CLOSE");
    }

    #endregion

    /// <summary>
    /// 計算チェックボックス切り替え時の処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ViewModelNotFoundException"></exception>
    /// <exception cref="NotFindItemException"></exception>
    private async void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            if (_vm is null) throw new ViewModelNotFoundException(nameof(BillingDataInquiryViewModel));
            if (sender as CheckBox is not CheckBox checkBox) throw new NotFindItemException($"{nameof(CheckBox_CheckedChanged)}に渡されたオブジェクトが[{nameof(CheckBox)}]ではありません。");
            if (checkBox.BindingContext as BillingDataInquiryGridViewDto is not BillingDataInquiryGridViewDto viewDto) throw new NotFindItemException($"{nameof(CheckBox)}にバインドされているオブジェクトが[{nameof(BillingDataInquiryGridViewDto)}]ではありません。");

            if (viewDto.OldCalcTarget != e.Value)  // 新旧の値が異なる場合のみ金額計算する（ユーザーがチェックボックスを切り替えた場合必ず値が異なるため）
            {
                _vm.CalcTotalAmount += e.Value ? viewDto.TotalAmount : viewDto.TotalAmount * -1;
                _vm.CalcBillingAmount += e.Value ? viewDto.BillingAmount : viewDto.BillingAmount * -1;
                viewDto.OldCalcTarget = e.Value;
            }
        }
        catch (Exception ex)
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(CheckBox_CheckedChanged), null);
            Application.Current!.MainPage = new ErrorPage();
        }
    }
}
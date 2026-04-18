using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using Expense_Fair_Split.Views.NewRegistrations;

namespace Expense_Fair_Split.Views.Login;

public partial class LoginMenuPage : ContentPage
{
    private Color? _originalBarBackgroundColor;
    private Color? _originalBarTextColor;
    private readonly IUserService _userService;
    private readonly ILogDataService _logDataService;
    private readonly UserSessionService _userSessionService;
    private readonly IServiceProvider _serviceProvider;

    public LoginMenuPage(IServiceProvider serviceProvider)
	{
		InitializeComponent();
        _userService = serviceProvider.GetRequiredService<IUserService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _serviceProvider = serviceProvider;
        this.Loaded += (_, _) =>
		{
			_vm = new LoginMenuViewModel();
			this.BindingContext = _vm;
		};
	}
	LoginMenuViewModel? _vm;

    #region Rendering Process
    /**** 初期表示処理 ****/
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Application.Current?.MainPage is NavigationPage navPage)
        {
            // 画面表示時のデフォルト色を保存
            _originalBarBackgroundColor = navPage.BarBackgroundColor;
            _originalBarTextColor = navPage.BarTextColor;

            // このページの固有色を設定
            navPage.BarBackgroundColor = CustomColor.SoftBlue;
            navPage.BarTextColor = Colors.White;
        }
    }

    /**** ページ非表示時処理 ****/
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (Application.Current?.MainPage is NavigationPage navPage)
        {
            // ページ表示時の色に戻す
            navPage.BarBackgroundColor = _originalBarBackgroundColor ?? CustomColor.Primary;
            navPage.BarTextColor = _originalBarTextColor ?? Colors.White;
        }
    }
    #endregion

    /// <summary>
    /// アカウント新規作成画面への遷移
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnNewRegistration(object sender, EventArgs e)
    {
        await this.Navigation.PushAsync(new NewRegistrationPage(_serviceProvider));
    }

    /// <summary>
    /// ユーザーログインチェック＆ログイン処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void InputCheckAndUserLogin(object sender, EventArgs e)
    {
        if (Application.Current?.MainPage is NavigationPage navPage)
        {
            if (_vm is not null)
            {
                _vm.ErrMsg = string.Empty;
                bool showNextScreen = true;

                /*** 画面入力チェックStart ***/
                (string ErrMsg, bool IsChecked) strEmptyValid = CommonUtil.StrNullOrEmptyCheck(new List<string> { _vm.EmailAddress, _vm.Password });
                if (!strEmptyValid.IsChecked)
                {
                    _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                    _vm.ErrMsg += $"・{Properties.Resources.ItemsEmpty}";
                    showNextScreen = false;
                }

                if (!showNextScreen)
                {
                    _vm.ShowErrMsg = true;
                    return;
                }
                /*** 画面入力チェックEnd ***/

                /*** DBとの存在チェックStart ***/
                User? loginUser = await _userService.GetUserByEMailAsync(_vm.EmailAddress);
                if (loginUser is null)
                {
                    _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                    _vm.ErrMsg += $"・{Properties.Resources.UserNotExsistsbyEmailAddess}";
                    _vm.ShowErrMsg = true;
                    return;
                }
                // 画面入力されたパスワード値もハッシュ化して比較
                if (!loginUser.VerifyPassword(_vm.Password)) 
                {
                    _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                    _vm.ErrMsg += $"・{Properties.Resources.NotEqualPassword}";
                    _vm.ShowErrMsg = true;
                    return;
                }

                /*** DBとの存在チェックEnd ***/

                _userSessionService.Login(loginUser.Id, loginUser.Name, loginUser.Email);
                Preferences.Set(MappingStrResource.LoggedInUserId, loginUser.Id);

                // ログイン値チェック後メインページへ遷移
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                // ViewModelがインスタンス化されていなかった場合の処理
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ErrorMsgResource.UndefinedVM, null, nameof(InputCheckAndUserLogin), null);
                Application.Current!.MainPage = new ErrorPage();
            }

        }
        else
        {
            // NavigationPageではなかった場合の処理
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ErrorMsgResource.MissingNavPage, null, nameof(InputCheckAndUserLogin), null);
            Application.Current!.MainPage = new ErrorPage();
        }
    }
}
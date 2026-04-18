using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Api;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using Microsoft.EntityFrameworkCore;
using SQLite;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Http.Json;

namespace Expense_Fair_Split.Views.NewRegistrations;

public partial class NewRegistrationPage : ContentPage
{
	private Color? _originalBarBackgroundColor;
    private Color? _originalBarTextColor;
    private readonly IUserService _userService;
    private readonly ILogDataService _logDataService;
    private readonly UserSessionService _userSessionService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ApiClient _apiClient;

    public NewRegistrationPage(IServiceProvider serviceProvider)
	{
		InitializeComponent();
        _userService = serviceProvider.GetRequiredService<IUserService>();
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
        _apiClient = serviceProvider.GetRequiredService<ApiClient>();
		_serviceProvider = serviceProvider;
        this.Loaded += (_, _) =>
		{
			_vm = new NewRegistrationViewModel();
			this.BindingContext = _vm;
		};
	}
	NewRegistrationViewModel? _vm;

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
		else 
		{
            // NavigationPageではなかった場合の例外処理等
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
        else
        {
            // NavigationPageではなかった場合の例外処理等
        }
    }
    #endregion

    /// <summary>
    /// ユーザー登録画面の入力値チェック後、パラメータ登録、メインメニューへ画面遷移
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void InputCheckAndUserRegister(object sender, EventArgs e)
	{
        try
        {
            if (Application.Current?.MainPage is NavigationPage navPage)
            {
                if (_vm is not null)
                {
                    if (_vm._isProcessing)
                    {
                        // 処理実行中に再度ボタンを押された場合はメッセージを表示し、何もせずリターン
                        _vm.ProcessMsg = $"・{Properties.Resources.isProcessing}";
                        _vm.ShowProcessMsg = true;
                        return;
                    }
                    _vm._isProcessing = true;

                    try
                    {
                        _vm.ErrMsg = string.Empty;
                        _vm.ShowErrMsg = false;
                        bool showNextScreen = true;
                        const int MaxCharLength = 15;

                        /*** 入力チェックStart ***/
                        (string ErrMsg, bool IsChecked, int ErrType) strLenValid = CommonUtil.IsStringMaxLengthValid(new List<string> { _vm.UserName }, MaxCharLength, false);
                        if (!strLenValid.IsChecked)
                        {
                            _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                            if (strLenValid.ErrType == 1)
                            {
                                _vm.ErrMsg += $"・お名前は{Properties.Resources.RequiredItem}";
                            }
                            else if (strLenValid.ErrType == 2)
                            {
                                _vm.ErrMsg += $"・お名前は{MaxCharLength}{Properties.Resources.TextCountExceeds}";
                            }
                            showNextScreen = false;
                        }

                        bool regexChecked = StringValids.StrRegexPatternSelect(new List<string> { _vm.UserName }, (int)EnumResource.RegexPatternSelect.JapaneseAndSingleByteAlphaNumeric, true, out string regErrMsg, out int regErrType);
                        if (!regexChecked)
                        {
                            _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                            _vm.ErrMsg += $"・お名前に{Properties.Resources.CharacterNotAvailable}";
                            showNextScreen = false;
                        }

                        // メールアドレスの検証
                        (bool IsChecked, int ErrType) emailChecked = StringValids.IsValidEmail(_vm.EmailAddress);
                        if (!emailChecked.IsChecked)
                        {
                            _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                            if (emailChecked.ErrType == 1)
                            {
                                _vm.ErrMsg += $"・メールアドレスは{Properties.Resources.RequiredItem}";
                            }
                            else if (emailChecked.ErrType == 2)
                            {
                                _vm.ErrMsg += "・" + Properties.Resources.EmailAddressNotRequirements;
                            }
                            showNextScreen = false;
                        }

                        // パスワードの検証
                        var validationContext = new ValidationContext(_vm);
                        var validationResults = new List<ValidationResult>();
                        bool isValidPass = Validator.TryValidateObject(_vm, validationContext, validationResults, validateAllProperties: true);
                        if (!isValidPass)
                        {
                            _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                            _vm.ErrMsg += $"・パスワードは8～64文字以内かつ、大文字、小文字、数字、特殊文字の中から3つ以上使う必要があります。";
                            showNextScreen = false;
                        }

                        if (!showNextScreen)
                        {
                            _vm.ShowErrMsg = true;
                            return;
                        }
                        /*** 入力チェックEnd ***/

                        bool answer = await DisplayAlert("確認", "画面の内容で登録を行ってもよろしいですか？", "OK", "Cancel");
                        if (!answer)
                        {
                            return;
                        }

                        /*** DB登録処理Start ***/

                        // パスワードのハッシュ化
                        try
                        {
                            _vm.SetHashPassword(_vm.PassWord);
                        }
                        catch (PasswordHashingException ex)
                        {
                            throw;
                        }

                        // 登録予定の名前が既に使われていないかチェック　後日追加

                        // 登録予定アドレスが既に使われていないかチェック
                        if (await _userService.GetUserByEMailAsync(_vm.EmailAddress) is not null)
                        {
                            _vm.ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(_vm.ErrMsg);
                            _vm.ErrMsg += $"・{Properties.Resources.UsedRegistrationEmailAddress}";
                            _vm.ShowErrMsg = true;
                            return;
                        }

                        // Userの登録
                        User newUser = new User
                        {
                            Name = _vm.UserName,
                            Email = _vm.EmailAddress,
                            PasswordHash = _vm.PassWord,
                            IsSynced = false
                        };

                        // SQLite（ローカル）への登録
                        await _userService.CreateUserAsync(newUser);

                        // PostgreSQLへの登録
                        try
                        {
                            var response = await _apiClient.PostAsync("api/User", newUser);
                            if (response.IsSuccessStatusCode)
                            {
                                // DBへの登録成功時の処理
                                newUser.IsSynced = true;  // PostgreSQLへの登録成功時は同期フラグを同期済みに変更
                                await _userService.UpdateUserAsync(newUser);
                            }
                            else
                            {
                                Debug.WriteLine("PostgreSQL へのユーザー登録に失敗しました。ローカルデータを削除します。");
                                throw new Exception("PostgreSQL へのユーザー登録に失敗しました。");
                            }
                        }
                        catch (Exception)
                        {
                            await _userService.DeleteUserAsync(newUser.Id);  // DBへの書き込み失敗時、ローカルに登録したユーザーを消す
                            throw;
                        }

                        User? loginUser = await _userService.GetUserByEMailAsync(_vm.EmailAddress);
                        if (loginUser is null)
                        {
                            throw new NotFindItemException($"登録時メールアドレスから({nameof(User)})が見つかりません。");
                        }

                        _userSessionService.Login(loginUser.Id, loginUser.Name, loginUser.Email);
                        Preferences.Set(MappingStrResource.LoggedInUserId, loginUser.Id);

                        /*** DB登録処理End ***/
                        Application.Current.MainPage = new AppShell();
                    }
                    finally
                    {
                        _vm._isProcessing = false;
                        _vm.ProcessMsg = string.Empty;
                        _vm.ShowProcessMsg = false;
                    }
                }
                else
                {
                    throw new ViewModelNotFoundException(nameof(NewRegistrationViewModel));
                }
            }
            else
            {
                // NavigationPageではなかった場合の処理
                throw new Exception();
            }
        }
        catch (Exception ex)
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, null, nameof(InputCheckAndUserRegister), null);
            Application.Current!.MainPage = new NavigationPage(new ErrorPage());
        }
    }
}
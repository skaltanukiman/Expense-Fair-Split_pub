using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.Views.Error;
using Expense_Fair_Split.Views.Login;
using System.Diagnostics;

namespace Expense_Fair_Split.ViewModels
{
    public class SettingViewModel : Prism.Mvvm.BindableBase
    {
        private readonly UserSessionService _userSessionService;
        private readonly ILogDataService _logDataService;
        public DelegateCommand LogoutActionCommand { get; }

        public SettingViewModel()
        {
            var serviceProvider = App.Services;
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();

            LogoutActionCommand = new DelegateCommand(OnLogoutButtonClicked);

            LoginUserName = _userSessionService.UserName;
            LoginUserEmail = _userSessionService.Email;
        }

        public string LoginUserName { get; set; } = string.Empty;
        public string LoginUserEmail { get; set; } = string.Empty;

        /// <summary>
        /// ログアウトボタン押下時の処理
        /// </summary>
        private void OnLogoutButtonClicked()
        {
            try
            {
                SessionUnity.SignOutAndClearSession();

                if (Application.Current?.MainPage is null)
                {
                    throw new PageNotFoundException(nameof(MainPage));
                }
                else 
                {
                    IServiceProvider serviceProvider = App.Services;
                    Application.Current.MainPage = new NavigationPage(new LoginMenuPage(serviceProvider));  // ログアウト後、ログイン画面に遷移
                }
            }
            catch (Exception ex)
            {
                _ = _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(OnLogoutButtonClicked), null);
                Application.Current!.MainPage = new ErrorPage();
            }
        }
    }
}

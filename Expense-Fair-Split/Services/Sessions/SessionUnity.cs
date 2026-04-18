using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Sessions
{
    public static class SessionUnity
    {
        /// <summary>
        /// セッションログアウト後にPreferencesの更新を行います。
        /// </summary>
        /// <exception cref="NotUserSessionException"></exception>
        /// <exception cref="Exception"></exception>
        public static void SignOutAndClearSession()
        {
            var serviceProvider = App.Services;
            UserSessionService _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();

            if (_userSessionService is null || _userSessionService.UserId == -1) throw new NotUserSessionException();
            _userSessionService.Logout();

            if (!_userSessionService.IsLoggedIn)
            {
                // セッションのログアウト状態を確認後Preferencesに-1をセット
                Preferences.Set(MappingStrResource.LoggedInUserId, -1);
            }
            else
            {
                throw new Exception("ログアウト時に不明なエラーが発生しました。");
            }
        }
    }
}

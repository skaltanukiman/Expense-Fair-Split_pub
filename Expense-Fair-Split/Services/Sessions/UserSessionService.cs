using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Sessions
{
    public class UserSessionService
    {
        public int UserId { get; private set; } = -1;
        public string UserName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;

        public bool IsLoggedIn => UserId != -1;  // UserIdの値が-1以外(ログイン状態)の場合true、-1(ログアウト状態)の場合false

        public void Login(int userId, string userName, string email)
        {
            UserId = userId;
            UserName = userName;
            Email = email;
        }

        public void Logout() 
        {
            UserId = -1;
            UserName = string.Empty;
            Email = string.Empty;
        }
    }
}

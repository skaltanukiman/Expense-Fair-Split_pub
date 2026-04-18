using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services
{
    public interface IUserService
    {
        /// <summary>
        /// IDでユーザーを検索、取得します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<User?> GetUserAsync(int id);

        /// <summary>
        /// EMailでユーザーを検索、取得します。
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<User?> GetUserByEMailAsync(string email);

        /// <summary>
        /// 全てのユーザーを取得します。
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<User>> GetAllUsersAsync();

        /// <summary>
        /// ユーザーを登録します。
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task CreateUserAsync(User user);

        /// <summary>
        /// ユーザーを更新します。
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task UpdateUserAsync(User user);

        /// <summary>
        /// IDで指定されたユーザーを削除します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteUserAsync(int id);
    }
}

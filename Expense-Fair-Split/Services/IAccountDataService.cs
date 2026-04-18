using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services
{
    public interface IAccountDataService
    {
        /// <summary>
        /// IDで検索、取得します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AccountData?> GetAccountDataAsync(int id);

        /// <summary>
        /// 勘定名で検索、取得します。
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns></returns>
        Task<AccountData?> GetAccountDataByAccountNameAsync(string accountName);

        /// <summary>
        /// 全てのAccountDataを取得します。
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AccountData>> GetAllAccountDataAsync();

        /// <summary>
        /// AccountDataを作成します。
        /// </summary>
        /// <param name="accountData"></param>
        /// <returns></returns>
        Task CreateAccountDataAsync(AccountData accountData);

        /// <summary>
        /// AccountDataを更新します。
        /// </summary>
        /// <param name="accountData"></param>
        /// <returns></returns>
        Task UpdateAccountDataAsync(AccountData accountData);

        /// <summary>
        /// 指定IDのAccountDataを削除します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAccountDataAsync(int id);
    }
}

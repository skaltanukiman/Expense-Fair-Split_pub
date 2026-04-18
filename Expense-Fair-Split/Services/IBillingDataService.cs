using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services
{
    public interface IBillingDataService
    {
        /// <summary>
        /// IDで検索、取得します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<BillingData?> GetBillingDataAsync(int id);

        /// <summary>
        /// FromUserCodeが一致する全ての明細データを取得します。
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        Task<IEnumerable<BillingData>> GetAllByFromUserCodeFindAsync(int userCode);

        /// <summary>
        /// ToUserCodeが一致する全ての明細データを取得します。
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        Task<IEnumerable<BillingData>> GetAllByToUserCodeFindAsync(int userCode);

        /// <summary>
        /// 全ての明細データを取得します。
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<BillingData>> GetAllBillingDataAsync();

        /// <summary>
        /// 明細データを作成します。
        /// </summary>
        /// <param name="billingData"></param>
        /// <returns></returns>
        Task CreateBillingDataAsync(BillingData billingData);

        /// <summary>
        /// 明細データを更新します。
        /// </summary>
        /// <param name="billingData"></param>
        /// <returns></returns>
        Task UpdateBillingDataAsync(BillingData billingData);

        /// <summary>
        /// 指定IDの明細データを削除します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteBillingDataAsync(int id);
    }
}

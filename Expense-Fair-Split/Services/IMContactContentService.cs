using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Expense_Fair_Split.Commons.EnumResource;

namespace Expense_Fair_Split.Services
{
    public interface IMContactContentService
    {
        /// <summary>
        /// keyで検索、取得します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MContactContent?> GetByKeyMContactContentAsync(int id);

        /// <summary>
        /// タイプが一致する全てのMContactContentを取得します。
        /// </summary>
        /// <param name="contactType"></param>
        /// <param name="orderKey">
        ///     0: 昇順
        ///     1: 降順
        ///     その他: 昇順
        /// </param>
        /// <returns></returns>
        Task<IEnumerable<MContactContent>> GetAllByContactTypeFindAsync(string contactType, OrderKey orderKey);

        /// <summary>
        /// 全てのMContactContentを取得します。
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MContactContent>> GetAllMContactContentAsync();
    }
}

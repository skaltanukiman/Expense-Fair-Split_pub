using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services
{
    public interface IMDistRatioService
    {
        /// <summary>
        /// keyで検索、取得します。
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<MDistRatio?> GetByKeyMDistRatioAsync(int typeCode, int code);

        /// <summary>
        /// タイプコードが一致する全てのMDistRatioを取得します。
        /// </summary>
        /// <param name="typeCode"></param>
        /// <returns></returns>
        Task<IEnumerable<MDistRatio>> GetAllByRatioTypeCodeFindAsync(int typeCode);

        /// <summary>
        /// 全てのMDistRatioを取得します。
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MDistRatio>> GetAllMDistRatioAsync();

        /// <summary>
        /// MDistRatioを作成します。
        /// </summary>
        /// <param name="mDistRatio"></param>
        /// <returns></returns>
        Task CreateMDistRatioAsync(MDistRatio mDistRatio);

        /// <summary>
        /// MDistRatioを更新します。
        /// </summary>
        /// <param name="mDistRatio"></param>
        /// <returns></returns>
        Task UpdateMDistRatioAsync(MDistRatio mDistRatio);

        /// <summary>
        /// 指定IDのMDistRatioを削除します。
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task DeleteMDistRatioAsync(int typeCode, int code);
    }
}

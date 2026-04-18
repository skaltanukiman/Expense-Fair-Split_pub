using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services
{
    public interface ILogDataService
    {
        /// <summary>
        /// IDでログを検索、取得します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<LogData?> GetLogDataAsync(int id);

        /// <summary>
        /// 全てのログを取得します。
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<LogData>> GetAllLogDataAsync();

        /// <summary>
        /// ログを登録します。
        /// </summary>
        /// <param name="logData"></param>
        /// <returns></returns>
        Task CreateLogDataAsync(LogData logData);

        /// <summary>
        /// ログを更新します。
        /// </summary>
        /// <param name="logData"></param>
        /// <returns></returns>
        Task UpdateLogDataAsync(LogData logData);

        /// <summary>
        /// IDで指定されたログを削除します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteLogDataAsync(int id);

        /// <summary>
        /// ローカルとサーバーDBにログデータを登録します。
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <param name="userId"></param>
        /// <param name="source"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        Task InsertLog(string logLevel, string? message = null, int? userId = null, string? source = null, object? extraData = null);
    }
}

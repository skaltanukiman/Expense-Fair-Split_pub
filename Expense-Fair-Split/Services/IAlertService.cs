using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services
{
    public interface IAlertService
    {
        /// <summary>
        /// 画面にポップアップを表示します。
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">本文</param>
        /// <param name="accept">真を返すUI表示文字列</param>
        /// <param name="cancel">偽を返すUI表示文字列</param>
        /// <returns>
        ///     true: accept文字列を押下
        ///     false: cancel文字列を押下
        /// </returns>
        Task<bool> ShowAlertAsync(string title, string message, string accept, string cancel);
    }
}

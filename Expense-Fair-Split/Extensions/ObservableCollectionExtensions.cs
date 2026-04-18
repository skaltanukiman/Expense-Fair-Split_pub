using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Extensions
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// ObservableCollectionに定義されたIDを軸に並び替えを行います。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">並び替え対象</param>
        /// <param name="idSelector">並び替えの基準となるパラメータ</param>
        /// <param name="descending">
        ///     true: 降順
        ///     false: 昇順
        /// </param>
        public static void SortById<T>(this ObservableCollection<T> collection, Func<T, int> idSelector, bool descending = false)
        {
            // 渡されたidSelectorを元に並び替えを行う
            List<T> sortedList = descending
                ? collection.OrderByDescending(idSelector).ToList()  // 降順
                : collection.OrderBy(idSelector).ToList();           // 昇順

            // ObservableCollectionではOrderByのみでは動かないため一度クリアし再追加する。
            collection.Clear();
            foreach (T item in sortedList) 
            {
                collection.Add(item);
            }
        }
    }
}

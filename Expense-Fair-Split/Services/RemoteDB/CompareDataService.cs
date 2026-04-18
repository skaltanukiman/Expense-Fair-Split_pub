using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.RemoteDB
{
    public class CompareDataService
    {
        private readonly AppDbContext _context;

        public CompareDataService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// サーバーDBに存在するがローカルDBに存在しないデータを抽出し存在しないものだけをオブジェクトリストに格納します。
        /// </summary>
        /// <param name="serverDataListObject">サーバーDBのデータ</param>
        /// <param name="CompareLocalDBset">比較する対象のDBSetを指定するパラメータ</param>
        /// <returns>
        ///     true: 差分あり
        ///     false: 差分なし or エラー
        ///     
        ///     List: 差分のデータのみが入ったリスト
        /// </returns>
        public async Task<(bool, List<object>)> CompareAndExtractNewDataAsync(List<object> serverDataListObject, Type CompareLocalDBset)
        {
            List<object> resultList = new List<object>();

            foreach (object data in serverDataListObject)
            {
                if (data.GetType() != CompareLocalDBset) return (false, resultList);
            }

            if (CompareLocalDBset.Name == "AccountData")
            {
                List<AccountData> serverDataList = serverDataListObject.Cast<AccountData>().ToList();
                List<AccountData> localDataList = await _context.AccountDataSet.ToListAsync();

                List<AccountData> localMissingData = serverDataList.Where(serverData => !localDataList.Any(localData => localData.AccId == serverData.AccId)).ToList();

                resultList = localMissingData.Cast<object>().ToList();
            }
            else if (CompareLocalDBset.Name == "User")
            {
                List<User> serverDataList = serverDataListObject.Cast<User>().ToList();
                List<User> localDataList = await _context.Users.ToListAsync();

                List<User> localMissingData = serverDataList.Where(serverData => !localDataList.Any(localData => localData.Id == serverData.Id)).ToList();

                resultList = localMissingData.Cast<object>().ToList();
            }
            else if (CompareLocalDBset.Name == "BillingData")
            {
                List<BillingData> serverDataList = serverDataListObject.Cast<BillingData>().ToList();
                List<BillingData> localDataList = await _context.BillingDataSet.ToListAsync();

                List<BillingData> localMissingData = serverDataList.Where(serverData => !localDataList.Any(localData => localData.BillingNo == serverData.BillingNo)).ToList();

                resultList = localMissingData.Cast<object>().ToList();
            }
            else if (CompareLocalDBset.Name == "LogData")
            {
                List<LogData> serverDataList = serverDataListObject.Cast<LogData>().ToList();
                List<LogData> localDataList = await _context.LogDataSet.ToListAsync();

                List<LogData> localMissingData = serverDataList.Where(serverData => !localDataList.Any(localData => localData.Id == serverData.Id)).ToList();

                resultList = localMissingData.Cast<object>().ToList();
            }
            else if (CompareLocalDBset.Name == "MContactContent")
            {
                List<MContactContent> serverDataList = serverDataListObject.Cast<MContactContent>().ToList();
                List<MContactContent> localDataList = await _context.MContactContentSet.ToListAsync();

                List<MContactContent> localMissingData = serverDataList.Where(serverData => !localDataList.Any(localData => localData.Id == serverData.Id)).ToList();

                resultList = localMissingData.Cast<object>().ToList();
            }
            else 
            {
                Debug.WriteLine($"指定されたDBsetが見つかりませんでした。[{CompareLocalDBset.Name}]");
                return (false, resultList);
            }

            if (resultList.Count == 0) return (false, resultList);

            return (true, resultList);
        }
    }
}

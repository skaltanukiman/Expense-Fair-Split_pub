using Expense_Fair_Split.Data;
using Expense_Fair_Split.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Expense_Fair_Split.Services.Impl
{
    public class SyncDataService : ISyncDataService
    {
        private readonly ISyncDataRepository _repository;
        private readonly AppDbContext _context;

        public SyncDataService(ISyncDataRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task UpdateDataAsync<T>(T data) where T : class
        {
            try
            {
                await _repository.UpdateAsync(data);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);
                _context.ChangeTracker.Clear();
                throw;
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }            
        }

        /// <summary>
        /// dataオブジェクトのIsSyncedプロパティをchangeValの値に更新します。
        /// </summary>
        /// <typeparam name="T">dataの型パラメータ</typeparam>
        /// <param name="data">更新対象オブジェクト</param>
        /// <param name="changeVal">true or false</param>
        /// <returns>
        ///     true: 成功
        ///     false: 失敗
        /// </returns>
        public async Task<bool> UpdateSyncedColumn<T>(T data, bool changeVal) where T : class
        {
            try
            {
                // ローカルDBのIsSyncedを引数パラメータの値に更新
                PropertyInfo? isSyncedProperty = data.GetType().GetProperty(Properties.Resources.SyncColumnStr);
                if (isSyncedProperty is not null && isSyncedProperty.CanWrite)
                {
                    isSyncedProperty.SetValue(data, changeVal);

                    await UpdateDataAsync<T>(data);

                    return true;
                }
                else
                {
                    Debug.WriteLine($"プロパティが取得できませんでした。");
                    return false;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine($"{nameof(UpdateSyncedColumn)}内で不正な処理が発生しました。");
                return false;
            }
        }
    }
}

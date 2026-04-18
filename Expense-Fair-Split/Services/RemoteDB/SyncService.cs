using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Models.Interface;
using Expense_Fair_Split.Services.Impl;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Maui.Data;
using Syncfusion.Maui.DataSource.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.RemoteDB
{
    public class SyncService
    {
        private readonly AppDbContext _context;
        private readonly RemoteDbService _remoteDbService;
        private readonly CompareDataService _compareDataService;
        private readonly SyncFlagService _syncFlagService;
        private readonly List<Type> syncDbSetType = SyncDBSetResource.syncDbSetType;
        private readonly List<Type> serverDbSetType = SyncDBSetResource.serverDbSetType;

        public SyncService(AppDbContext dbContext, RemoteDbService remoteDbService, CompareDataService compareDataService, SyncFlagService syncFlagService) 
        {
            _context = dbContext;
            _remoteDbService = remoteDbService;
            _compareDataService = compareDataService;
            _syncFlagService = syncFlagService;
        }

        #region public method

        /// <summary>
        /// ジェネリクスで受け取ったデータセットの中からローカルDBと未同期のデータを抽出し、Listで返します。
        /// </summary>
        /// <typeparam name="T">DBデータセットオブジェクト</typeparam>
        /// <returns>
        ///     正常: 未同期データをリストでリターン
        ///     エラー時: null
        /// </returns>
        public async Task<List<T>?> GetSyncTargetData<T>() where T : class
        {
            try
            {
                return await _context.Set<T>().Where(data => EF.Property<bool>(data, Properties.Resources.SyncColumnStr) == false).ToListAsync();
            }
            catch (Exception)
            {
                Debug.WriteLine($"{nameof(GetSyncTargetData)}内でエラーが発生しました。");
                return null;
            }
        }

        public async Task<bool> AddServerDataToLocal<T>(List<object> insertData) where T : class, IHasSyncStatus
        {

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                List<T> addDataList = insertData.OfType<T>().ToList();  // Listの型をobjectから登録DBSetの型に変換する

                addDataList.ForEach(data =>
                {
                    // UTC ⇒ JST
                    if (data is IHasTimeStamp timeStampedData)
                    {
                        timeStampedData.CreateDate = (DateTime)TimeConversion.ConvertUtcToJst(timeStampedData.CreateDate)!;
                        timeStampedData.UpdateDate = TimeConversion.ConvertUtcToJst(timeStampedData.UpdateDate);
                    }

                    if (data is IHasBillingDate timeStampedData2)
                    {
                        timeStampedData2.BillingDate = (DateTime)TimeConversion.ConvertUtcToJst(timeStampedData2.BillingDate)!;
                    }
                    data.IsSynced = true;
                });

                _context.Set<T>().AddRange(addDataList);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 登録～コミット後に時間をUTC基準に戻す（更新処理にて時間が二重で変換されることを防ぐため）
                addDataList.ForEach(data =>
                {
                    // JST ⇒ UTC
                    if (data is IHasTimeStamp timeStampedData)
                    {
                        timeStampedData.CreateDate = (DateTime)TimeConversion.ConvertJstToUtc(timeStampedData.CreateDate)!;
                        timeStampedData.UpdateDate = TimeConversion.ConvertJstToUtc(timeStampedData.UpdateDate);
                    }

                    if (data is IHasBillingDate timeStampedData2)
                    {
                        timeStampedData2.BillingDate = (DateTime)TimeConversion.ConvertJstToUtc(timeStampedData2.BillingDate)!;
                    }
                });

                return true;
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                Debug.WriteLine(ex.Message);
                _context.ChangeTracker.Clear();
                return false;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// ①ローカル → サーバー
        /// ②サーバー → ローカル
        /// の順に同期処理を実行します。
        /// </summary>
        /// <returns>
        ///     bool: 同期処理成功/失敗
        ///     string: 処理終了時間 (= 処理結果メッセージ出力時間)
        ///     string: 処理結果メッセージ
        /// </returns>
        public async Task<(bool, string, string)> RunFullSyncAsync()
        {
            (bool isSuccess, string msgTime, string msg) result = (false, "", "");

            try
            {
                _syncFlagService.ActivateSyncBlock();  // 同期処理のブロック

                // ローカルDB → サーバーDB同期処理を実行
                // ・他のユーザー等により先に該当IDのデータを作成されていた場合などは？
                // 　　⇒ 現段階では後勝ちとなる（後に同期されたもので上書きされる）
                // 　　⇒ 同期を定期実行だけでなくデータ登録の前等にも行う必要あり？
                // ・同期された時間は別でSyncedDateTimeのような形で持つ？
                bool syncSuccess1 = await SyncToDBServerAsync();
                if (!syncSuccess1)
                {
                    result.isSuccess = false;
                    result.msgTime = DateTime.Now.ToString();
                    result.msg = Properties.Resources.SyncFailedLocalToServer;

                    Debug.WriteLine($"{result.msgTime} {Properties.Resources.SyncFailedLocalToServer}");
                    return result;
                }

                // サーバー → ローカルDB同期処理を実行
                bool syncSuccess2 = await SyncServerDataToLocalAsync();
                if (!syncSuccess2)
                {
                    result.isSuccess = false;
                    result.msgTime = DateTime.Now.ToString();
                    result.msg = Properties.Resources.SyncFailedServerToLocal;

                    Debug.WriteLine($"{result.msgTime} {Properties.Resources.SyncFailedServerToLocal}");
                    return result;
                }

                result.isSuccess = true;
                result.msgTime = DateTime.Now.ToString();
                result.msg = Properties.Resources.SyncSuccess;

                Debug.WriteLine($"{result.msgTime} {Properties.Resources.SyncSuccess}");

                return result;
            }
            finally 
            {
                _syncFlagService.DeactivateSyncBlock();  // 同期処理の解放
            }
        }

        #endregion

        #region From Local To Server

        /// <summary>
        /// ローカルDBからサーバーDBへの未同期データを取得～同期処理までのメインタスク
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SyncToDBServerAsync()
        {
            Dictionary<string, List<object>> syncTargetDictionary = new Dictionary<string, List<object>>();  // 未同期データを格納するディクショナリ  key: className, val: classオブジェクトリスト

            Debug.WriteLine("ローカルからサーバーへの同期処理を開始します。");

            try
            {
                bool? isCreateDataDic = await CreateSyncDataDictionary(syncTargetDictionary);

                if (isCreateDataDic is null) return false;   // エラーの場合はfalseでリターン
                if (!isCreateDataDic.Value) return true;  // 未同期データなしの場合そのままtrueでリターン

                // 未同期データあり
                Debug.WriteLine("CreateDic");

                (bool isSuccessed, int syncedCount, int successCount) createResult = await _remoteDbService.SyncCreateDataAsync(syncTargetDictionary, syncDbSetType);
                if (!createResult.isSuccessed)
                {
                    Debug.WriteLine($"{nameof(RemoteDbService.SyncCreateDataAsync)}内で不正な処理が発生したため同期処理を中断します。");
                    return false;
                }

                (bool isSuccessed, int syncedCount, int successCount) putResult = await _remoteDbService.SyncPutDataAsync(syncTargetDictionary, syncDbSetType);
                if (!putResult.isSuccessed)
                {
                    Debug.WriteLine($"{nameof(RemoteDbService.SyncPutDataAsync)}内で不正な処理が発生したため同期処理を中断します。");
                    return false;
                }

                Debug.WriteLine($"{DateTime.Now.ToString()} ローカルDBからサーバーDBへの同期処理が完了しました。");
                Debug.WriteLine($"同期データ数: {createResult.syncedCount + putResult.syncedCount}");
                Debug.WriteLine($"処理成功件数: {createResult.successCount + putResult.successCount}");

                return true;
            }
            catch (Exception) 
            {
                Debug.WriteLine($"In {nameof(SyncToDBServerAsync)} is Exception");
                return false;
            }
        }

        #endregion

        #region From Server To Local

        /// <summary>
        /// サーバーDBからローカルDBへのデータを取得～同期処理までのメインタスク
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SyncServerDataToLocalAsync()
        {
            Dictionary<string, List<object>> serverDataDictionary = new Dictionary<string, List<object>>();  // サーバーデータを格納するディクショナリ  key: className, val: classオブジェクトリスト

            Debug.WriteLine("サーバーからローカルへの同期処理を開始します。");

            try
            {
                bool isSuccess = await CreateAllServerDataDictionary(serverDataDictionary);

                if (!isSuccess) return false;  // サーバーからデータが取得できなかった場合等はそのままリターン

                // 登録処理
                List<string> createFailedList = new List<string>();
                await SyncCreateDataToLocalAsync(serverDataDictionary, createFailedList);

                if (createFailedList.Count != 0)
                {
                    // 登録処理に失敗したデータセットがある場合の処理
                    Debug.WriteLine($"サーバーデータのローカルDBへの登録に失敗しました。");
                    createFailedList.ForEach(x => Debug.WriteLine(x));
                    return false;
                }

                // 更新処理
                bool isSuccess2 =  await SyncUpdateDataToLocalAsync(serverDataDictionary);
                if (!isSuccess2)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine($"In {nameof(SyncServerDataToLocalAsync)} is Exception");
                return false;
            }
        }

        #endregion

        #region Local CRUD

        private async Task<bool> SyncUpdateDataToLocalAsync(Dictionary<string, List<object>> serverDataDictionary)
        {
            foreach (Type type in serverDbSetType)
            {
                if (serverDataDictionary.TryGetValue(type.Name, out List<object>? serverDataListObject))
                {
                    foreach (object serverData in serverDataListObject)
                    {
                        MethodInfo? method = typeof(SyncService).GetMethod(nameof(LocalDataUpdateAsync), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.MakeGenericMethod(type);
                        Task? task = method!.Invoke(this, new object[] { serverData, type }) as Task;
                        await task!.ConfigureAwait(false);

                        bool updateResult = (bool)task!.GetType().GetProperty("Result")?.GetValue(task)!;

                        if (!updateResult)
                        {
                            // 更新失敗時の処理
                            Debug.WriteLine("サーバーからローカルへのデータ更新処理に失敗しました。");
                            return false;
                        }
                    }
                }
                else
                {
                    // ディクショナリーキー指定DBSetが格納されていなかった場合の処理
                    Debug.WriteLine($"{nameof(SyncUpdateDataToLocalAsync)}内のディクショナリーにおいてキーデータが見つかりませんでした。");
                }
            }
            return true;
        }

        private async Task<bool> LocalDataUpdateAsync<T>(T serverData, Type dbSetType) where T : class, IHasSyncStatus
        {
            try
            {
                if (serverData is IHasTimeStamp hasTimeStampData)
                {
                    hasTimeStampData.CreateDate = (DateTime)TimeConversion.ConvertUtcToJst(hasTimeStampData.CreateDate)!;
                    hasTimeStampData.UpdateDate = TimeConversion.ConvertUtcToJst(hasTimeStampData.UpdateDate);
                }

                if (serverData is IHasBillingDate hasTimeStampData2)
                {
                    hasTimeStampData2.BillingDate = (DateTime)TimeConversion.ConvertUtcToJst(hasTimeStampData2.BillingDate)!;
                }

                if (serverData is IHasTimeStamp2 hasTimeStampData3)
                {
                    hasTimeStampData3.Timestamp = (DateTime)TimeConversion.ConvertUtcToJst(hasTimeStampData3.Timestamp)!;
                }
                serverData.IsSynced = true;

                _context.ChangeTracker.Clear();  // ローカルには強制的にサーバー上のデータに更新する（既にローカルデータはサーバーに同期されているはずなのでサーバーが正とする）
                _context.Set<T>().Update(serverData);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);
                _context.ChangeTracker.Clear();
                
                return false;
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task SyncCreateDataToLocalAsync(Dictionary<string, List<object>> serverDataDictionary, List<string> createFailedList)
        {
            foreach (Type type in serverDbSetType)
            {
                if (serverDataDictionary.TryGetValue(type.Name, out List<object>? serverDataListObject))
                {
                    (bool hasExtractedData, List<object> localMissingDataList) result = await _compareDataService.CompareAndExtractNewDataAsync(serverDataListObject, type);

                    if (!result.hasExtractedData) continue;

                    // サーバー上のデータにローカル未登録データがあった場合のみ、登録処理
                    MethodInfo? method = typeof(SyncService).GetMethod(nameof(AddServerDataToLocal))!.MakeGenericMethod(type);
                    Task? task = method.Invoke(this, new object[] { result.localMissingDataList }) as Task;
                    await task!.ConfigureAwait(false);

                    bool registerResult = (bool)task!.GetType().GetProperty("Result")?.GetValue(task)!;

                    if (!registerResult)
                    {
                        createFailedList.Add(type.Name);
                    }
                }
                else
                {
                    // ディクショナリーキー指定DBSetが格納されていなかった場合の処理
                    Debug.WriteLine($"{nameof(SyncCreateDataToLocalAsync)}内のディクショナリーにおいてキーデータが見つかりませんでした。");
                }
            }
        }

        #endregion

        #region private method

        /// <summary>
        /// 引数で渡されたディクショナリーにサーバーDBからserverDbSetType指定テーブル毎の全データを格納します。
        /// </summary>
        /// <param name="serverDataDictionary"></param>
        /// <returns></returns>
        private async Task<bool> CreateAllServerDataDictionary(Dictionary<string, List<object>> serverDataDictionary)
        {
            try
            {
                foreach (Type type in serverDbSetType)
                {
                    MethodInfo? method = typeof(RemoteDbService).GetMethod(nameof(RemoteDbService.CreateServerAllDataDictionary))!.MakeGenericMethod(type);
                    Task? task = method.Invoke(_remoteDbService, new object[] { serverDataDictionary, type }) as Task;
                    await task!.ConfigureAwait(false);

                    bool? isCreateDataDic = (bool)task!.GetType().GetProperty("Result")?.GetValue(task)!;

                    if (isCreateDataDic is null)
                    {
                        Debug.WriteLine($"{nameof(RemoteDbService.CreateServerAllDataDictionary)}の返り値がnullです。Type: {type.Name}");
                    }
                    else if (!(bool)isCreateDataDic)
                    {
                        Debug.WriteLine($"該当DBセットの格納されたデータはありません。Type: {type.Name}");
                    }
                }

                if (serverDataDictionary.Count == 0)
                {
                    Debug.WriteLine($"サーバーから取得したデータはありません。");
                    return false;
                }

                return true;
            }
            catch (Exception) 
            {
                Debug.WriteLine($"In {nameof(CreateAllServerDataDictionary)} is Exception");
                return false;
            }
        }

        /// <summary>
        /// 引数で渡したディクショナリーにDBセット毎に未同期データを格納します。
        /// </summary>
        /// <param name="dic"></param>
        /// <returns>
        ///     true: 格納あり
        ///     false: 格納なし（未同期データなし）
        ///     null: エラー
        /// </returns>
        private async Task<bool?> CreateSyncDataDictionary(Dictionary<string, List<object>> dic)
        {
            try
            {
                foreach (Type type in syncDbSetType)
                {
                    // Typeをジェネリックに渡しメソッドを実行する
                    MethodInfo? method = typeof(SyncService).GetMethod(nameof(GetSyncTargetData))!.MakeGenericMethod(type);
                    Task? task = method.Invoke(this, null) as Task;
                    await task!.ConfigureAwait(false);

                    // 取得した未同期データを、リストに格納する。
                    PropertyInfo? resultProperty = task.GetType().GetProperty("Result");
                    IList? result = resultProperty?.GetValue(task) as IList;

                    if (result is not null && result.Count >= 1)
                    {
                        List<object> syncData = new List<object>();
                        syncData = result.Cast<object>().ToList();

                        // Type毎に[key: Type名、val: 未同期データ]の形でディクショナリーに格納する。
                        dic.Add(type.Name, syncData);
                    }
                }
                return dic.Count >= 1 ? true : false;
            }
            catch (ArgumentNullException)
            {
                Debug.WriteLine($"In {nameof(CreateSyncDataDictionary)} Parameter is null");
                return null;
            }
            catch (Exception)
            {
                Debug.WriteLine($"In {nameof(CreateSyncDataDictionary)} is Exception");
                return null;
            }
        }

        #endregion
    }
}

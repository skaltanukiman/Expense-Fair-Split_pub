using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Services.Api;
using Expense_Fair_Split.Services.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Expense_Fair_Split.Commons.SetPropertiesResource;

namespace Expense_Fair_Split.Services.RemoteDB
{
    public class RemoteDbService
    {
        private readonly ApiClient _apiClient;
        private readonly AppDbContext _dbContext;
        private readonly ISyncDataService _syncDataService;

        public RemoteDbService(ApiClient apiClient, AppDbContext dbContext, ISyncDataService syncDataService)
        {
            _apiClient = apiClient;
            _dbContext = dbContext;
            _syncDataService = syncDataService; 
        }

        #region Local To Server CRUD

        /// <summary>
        /// ローカルDBの未同期データをサーバーDBに同期（登録）します。
        /// </summary>
        /// <param name="syncDataDic">未同期データが入ったディクショナリー</param>
        /// <param name="syncDbSetType">同期対象DBSetを指定するType型パラメータが入ったリスト</param>
        /// <returns>
        ///     bool: (true: 正常終了、false: 異常終了)
        ///     int: データ同期処理を実行した回数
        ///     int: データ同期が成功した回数
        /// </returns>
        public async Task<(bool, int, int)> SyncCreateDataAsync(Dictionary<string, List<object>> syncDataDic, List<Type> syncDbSetType)
        {
            int syncedCount = 0;  // データ同期処理を実行した回数を記録するカウンタ
            int successCount = 0;  // データ同期が成功した回数を記録するカウンタ

            if (syncDataDic is null || syncDataDic.Count == 0) return (true, syncedCount, successCount);

            try
            {
                foreach (Type type in syncDbSetType)
                {
                    if (syncDataDic.TryGetValue(type.Name, out List<object>? syncDataList))
                    {
                        int deleteNum = 0;  // リスト内データ要素の削除対応によるインデックス指定用数値
                        string targetDbFindKey = string.Empty;  // キー列名を格納する変数
                        int keyType = 0;  // 列名の型タイプ

                        bool result = GetDbSearchKey(type.Name, ref targetDbFindKey, ref keyType);

                        if (!result)
                        {
                            Debug.WriteLine($"{nameof(targetDbFindKey)}を取得できませんでした。");
                            return (false, syncedCount, successCount);
                        }

                        int loopNum = syncDataList.Count;  // リスト内要素の削除が発生してもループ回数は変えたくないため、予め変数に回数を格納しておく
                        // リスト内データ削除に対応のためforeachからfor文に置き換え
                        for (int i = 0; i < loopNum; i++)
                        {
                            object data = syncDataList[i - deleteNum];

                            if (data.GetType() == type)
                            {
                                PropertyInfo? prop = data.GetType().GetProperty(targetDbFindKey);  // dataオブジェクトのプロパティ情報を取得
                                if (prop is null)
                                {
                                    Debug.WriteLine($"{nameof(prop)}を取得できませんでした。");
                                    return (false, syncedCount, successCount);
                                }

                                Type? genericType = TypeSelection.SelectByNumber(keyType);
                                if (genericType is null)
                                {
                                    Debug.WriteLine($"入力された{nameof(keyType)}が不正な値です。");
                                    return (false, syncedCount, successCount);
                                }

                                // dataオブジェクトと同一のデータがサーバーDBに存在するかを確認
                                MethodInfo? method = typeof(RemoteDbService)
                                    .GetMethod(nameof(CheckDataExistsByKey), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
                                    .MakeGenericMethod(genericType!);
                                Task? task = method!.Invoke(this, new object[] { prop, data, type }) as Task;
                                await task!.ConfigureAwait(false);

                                bool dataExists = (bool)task!.GetType().GetProperty("Result")?.GetValue(task)!;

                                if (dataExists)
                                {
                                    // データが存在した場合、何もせず次のデータへ、登録ではなく更新処理の方で処理をする。
                                    continue;
                                }
                                else
                                {
                                    syncedCount++;

                                    HttpResponseMessage response = await _apiClient.PostAsync($"api/{type.Name}/sync", data);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        successCount++;

                                        // 登録対象データのIsSynced列を同期済みに更新
                                        MethodInfo? syncedUpdateMethod = typeof(SyncDataService)
                                            .GetMethod(nameof(SyncDataService.UpdateSyncedColumn))?
                                            .MakeGenericMethod(type);

                                        Task? task2 = syncedUpdateMethod!.Invoke(_syncDataService, new object[] { data, true }) as Task;
                                        await task2!.ConfigureAwait(false);

                                        bool changeSuccess = (bool)task2!.GetType().GetProperty("Result")?.GetValue(task2)!;
                                        if (!changeSuccess)
                                        {
                                            // 失敗した場合はログだけ残す
                                            Debug.WriteLine($"IsSynced列の更新に失敗しました。");
                                        }

                                        // リストから登録対象のデータを消す（更新処理の要素から外すため）
                                        // 消した場合、要素が前に詰まるので削除用カウンタ変数を進める
                                        syncDataList.RemoveAt(i - deleteNum);
                                        deleteNum++;
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"登録に失敗しました。");
                                        return (false, syncedCount, successCount);
                                    }
                                }
                            }
                        }
                    }
                }
                return (true, syncedCount, successCount);
            }
            catch (Exception)
            {
                Debug.WriteLine($"{nameof(SyncPutDataAsync)}でDB同期中にエラーが発生しました。");
                return (false, syncedCount, successCount);
            }
        }

        /// <summary>
        /// ローカルDBの未同期データをサーバーDBに同期（更新）します。
        /// </summary>
        /// <param name="syncDataDic">未同期データが入ったディクショナリー</param>
        /// <param name="syncDbSetType">同期対象DBSetを指定するType型パラメータが入ったリスト</param>
        /// <returns>
        ///     bool: (true: 正常終了、false: 異常終了)
        ///     int: データ同期処理を実行した回数
        ///     int: データ同期が成功した回数
        /// </returns>
        public async Task<(bool, int, int)> SyncPutDataAsync(Dictionary<string, List<object>> syncDataDic, List<Type> syncDbSetType)
        {
            int syncedCount = 0;  // データ同期処理を実行した回数を記録するカウンタ
            int successCount = 0;  // データ同期が成功した回数を記録するカウンタ

            if (syncDataDic is null || syncDataDic.Count == 0) return (true, syncedCount, successCount);

            try
            {
                foreach (Type type in syncDbSetType)
                {
                    Debug.WriteLine($"type: {type.Name}");
                    if (syncDataDic.TryGetValue(type.Name, out List<object>? syncDataList))
                    {
                        foreach (object data in syncDataList)
                        {
                            if (data.GetType() == type)
                            {
                                syncedCount++;

                                HttpResponseMessage response = await _apiClient.PutAsync($"api/{type.Name}/sync", data);

                                if (response.IsSuccessStatusCode)
                                {
                                    successCount++;

                                    // ローカルDBのIsSyncedを同期済みに更新
                                    PropertyInfo? isSyncedProperty = data.GetType().GetProperty(Properties.Resources.SyncColumnStr);
                                    if (isSyncedProperty is not null && isSyncedProperty.CanWrite)
                                    {
                                        isSyncedProperty.SetValue(data, true);

                                        MethodInfo? method = typeof(SyncDataService)
                                            .GetMethod(nameof(SyncDataService.UpdateDataAsync))?
                                            .MakeGenericMethod(data.GetType());

                                        Task? task = method!.Invoke(_syncDataService, new object[] { data }) as Task;
                                        await task!.ConfigureAwait(false);
                                    }
                                }
                                else 
                                {
                                    // API通信、更新処理失敗時
                                    Debug.WriteLine($"更新処理に失敗しました。");
                                    return (false, syncedCount, successCount);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{nameof(SyncPutDataAsync)}でDB同期中にエラーが発生しました。");
                return (false, syncedCount, successCount);
            }
            
            return (true, syncedCount, successCount);
        }

        #endregion

        #region private method

        /// <summary>
        /// API経由でキーによる検索を行いデータが存在するかを確認する
        /// </summary>
        /// <typeparam name="T">Keyの型</typeparam>
        /// <param name="prop">Keyの列名情報を持つPropertyInfo</param>
        /// <param name="orgData">比較対象となる元データ</param>
        /// <param name="dbObjectType">検索するDbSetの型</param>
        /// <returns>
        ///     true: 存在あり
        ///     false: 存在なし
        ///     null: その他（エラー等）
        /// </returns>
        private async Task<bool?> CheckDataExistsByKey<T>(PropertyInfo prop, object orgData, Type dbObjectType) where T : IConvertible
        {
            T? searchKeyVal = (T?)prop.GetValue(orgData);

            if (searchKeyVal is null)
            {
                Debug.WriteLine($"{nameof(searchKeyVal)}が取得できませんでした。");
                return null;
            }

            try
            {
                HttpResponseMessage response = await _apiClient.GetAsync($"api/{dbObjectType.Name}/{searchKeyVal}");
                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 404) return false;  // データが存在しない場合はfalse

                    Debug.WriteLine("不正なリクエストです。");
                    return null;  // その他の不正エラー等の場合はnull
                }

                return true;

                //string jsonData = await response.Content.ReadAsStringAsync();

                //JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                //{
                //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                //};
                //var j = JsonSerializer.Deserialize(jsonData, dbObjectType, serializeOptions);
            }
            catch (Exception ex) 
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// DBSetのキー列名を取得します。
        /// </summary>
        /// <param name="dbSetName">キー列名を取得するDBSet名文字列</param>
        /// <param name="targetDbFindKey">取得した列名</param>
        /// <param name="keyType">列タイプ</param>
        /// <returns>
        ///     true: 取得成功
        ///     false: 取得失敗
        /// </returns>
        private bool GetDbSearchKey(string dbSetName, ref string targetDbFindKey, ref int keyType)
        {
            // 追加対象DBのキー列名を取得（ServerDBに存在するデータかを確認するため）
            string getVal = GetDbSetIdName.Get(dbSetName);
            if (string.IsNullOrWhiteSpace(getVal))
            {
                Debug.WriteLine($"{nameof(GetDbSetIdName)}からパラメータを取得できませんでした。");
                return false;
            }

            try
            {
                string[] spValArray = getVal.Split('|');

                targetDbFindKey = spValArray[0];
                keyType = int.Parse(spValArray[1]);

                return true;
            }
            catch (FormatException)
            {
                Debug.WriteLine($"文字列を変換できない型に変換しようとしています。");
                return false;
            }
            catch (Exception)
            {
                Debug.WriteLine($"{nameof(GetDbSearchKey)}内でエラーが発生しました。");
                return false ;
            }
        }

        #endregion

        #region public method

        /// <summary>
        /// 引数で渡されたディクショナリーに、Tyoeパラメータで指定されたDBSetの全データを格納します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic">
        ///     key: DBSet名
        ///     val: データオブジェクト
        /// </param>
        /// <param name="serverDbSetType">抽出テーブル指定</param>
        /// <returns>
        ///     true: 抽出あり
        ///     false: 抽出なし
        /// </returns>
        public async Task<bool> CreateServerAllDataDictionary<T>(Dictionary<string, List<object>> dic, Type serverDbSetType) where T : class
        {
            if (serverDbSetType is null) return false;

            try
            {
                // 指定テーブル毎に全データ取得 & ディクショナリーに格納
                HttpResponseMessage response = await _apiClient.GetAsync($"api/{serverDbSetType.Name}");

                if (response.IsSuccessStatusCode)
                {
                    List<T>? dataList = await response.Content.ReadFromJsonAsync<List<T>>();
                    if (dataList is null || dataList.Count == 0) return false;  // null or 0件の場合は処理を抜ける

                    List<object> objList = dataList.Cast<object>().ToList();  // ディクショナリーに格納するためList<object>変換する（要素の型情報は保持）
                    
                    dic.Add(serverDbSetType.Name, objList);
                }
                else
                {
                    Debug.WriteLine($"{nameof(serverDbSetType.Name)}のデータ取得処理が完了できませんでした。");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine($"{nameof(CreateServerAllDataDictionary)}でDB通信中にエラーが発生しました。");
                return false;
            }
        }

        #endregion
    }
}

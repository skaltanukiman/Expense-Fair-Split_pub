using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Services.Impl;
using Expense_Fair_Split.Services.RemoteDB;
using Microsoft.Extensions.Hosting;

namespace Expense_Fair_Split.Services.Background
{
    public class SyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SyncBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Debug.WriteLine("SyncBackgroundService が開始されました！");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        SyncFlagService syncFlagService = scope.ServiceProvider.GetRequiredService<SyncFlagService>();

                        if (!syncFlagService.IsSyncFlagActive())
                        {
                            Debug.WriteLine($"{DateTime.Now.ToString()} 自動実行同期処理を開始します。");
                            try
                            {
                                SyncService syncService = scope.ServiceProvider.GetRequiredService<SyncService>();

                                // 同期処理実行
                                (bool isSuccess, string msgTime, string msg) syncResult = await syncService.RunFullSyncAsync();

                                if (!syncResult.isSuccess)
                                {
                                    throw new Exception(syncResult.msg);
                                }

                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"同期処理中にエラー: {ex.Message}");
                                ILogDataService logDataService = scope.ServiceProvider.GetRequiredService<ILogDataService>();
                                await logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), ex.Message, null, nameof(SyncBackgroundService), null);
                            }
                        }
                        else
                        {
                            Debug.WriteLine("自動実行の同期処理をスキップしました。");
                        }

                    }

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                App.IsSyncRunning = false;
                Trace.WriteLine($"{nameof(SyncBackgroundService)}: {nameof(ExecuteAsync)}の処理を停止しました。");
            }            
        }
    }
}

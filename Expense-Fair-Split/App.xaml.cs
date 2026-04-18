using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Api;
using Expense_Fair_Split.Services.Background;
using Expense_Fair_Split.Services.Impl;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.Views.Error;
using Expense_Fair_Split.Views.Login;
using Expense_Fair_Split.Views.NewRegistrations;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Expense_Fair_Split
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        public static bool IsSyncRunning { get; set; } = false;  // バックグラウンドで同期処理が動いているか
        public static CancellationTokenSource Cts { get; set; } = new CancellationTokenSource();
        private readonly IUserService _userService;
        private readonly UserSessionService _userSessionService;
        private readonly SyncBackgroundService _syncBackgroundService;
        private readonly ILogDataService _logDataService;
        private readonly ApiClient _apiClient;
        

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            _userService = serviceProvider.GetRequiredService<IUserService>();
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _syncBackgroundService = serviceProvider.GetRequiredService<SyncBackgroundService>();
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _apiClient = serviceProvider.GetRequiredService<ApiClient>();
            Services = serviceProvider;

            _apiClient.NewApiRequets();

            InitializeApp(serviceProvider);
        }

        #region Initialize Method

        private async void InitializeApp(IServiceProvider serviceProvider)
        {
            bool startSync = true;  // 同期処理の開始許可
            int userId = Preferences.Get(MappingStrResource.LoggedInUserId, -1);  // ユーザー情報がアプリケーションに保存されていない場合は-1

            try
            {
                SetInitialTheme();

                if (userId != -1)
                {
                    // ユーザー情報がPreferences上から取得できた場合、そのままメインメニューへ
                    User? loginUser = await _userService.GetUserAsync(userId);
                    if (loginUser is not null)
                    {
                        // セッションにユーザー情報を登録後画面遷移
                        _userSessionService.Login(loginUser.Id, loginUser.Name, loginUser.Email);
                        MainPage = new AppShell();
                    }
                    else
                    {
                        Preferences.Set(MappingStrResource.LoggedInUserId, -1);
                        throw new Exception($"{nameof(loginUser)}が取得出来ませんでした。");
                    }
                }
                else
                {
                    // ユーザー情報がPreferences上から取得できなかった場合、ログイン画面へ
                    MainPage = new NavigationPage(new LoginMenuPage(serviceProvider));
                }
            }
            catch (Exception ex) 
            {
                startSync = false;

                // ログ処理処理をバックグラウンドスレッドで実行（メインスレッドだとAPI側に遷移時エラーとなってしまうため）
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, null, nameof(InitializeApp), null);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error: {ex.Message}");
                    }
                });
                
                MainPage = new ErrorPage();
            }

            if (startSync) 
            {
                IsSyncRunning = true;

                // 定期実行同期処理をバックグラウンドスレッドで実行
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _syncBackgroundService.StartAsync(Cts.Token);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error: {ex.Message}");
                    }
                });
            }
        }

        #endregion

        #region Private Method

        /// <summary>
        /// アプリケーションテーマの初期設定を行います。
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void SetInitialTheme()
        {
            try
            {
#if ANDROID
                Application.Current!.UserAppTheme = AppTheme.Light;
#endif
            }
            catch (Exception)
            {
                throw new Exception($"({nameof(SetInitialTheme)})内でエラーが発生しました。");
            }
        }

#endregion

        #region StateChange Method

        /// <summary>
        /// アプリケーションがバックグラウンドに移行する時の処理
        /// </summary>
        protected override void OnSleep()
        {
            base.OnSleep();
            CancelBackgroundTask();
            Debug.WriteLine($"{DateTime.Now.ToString()} 同期処理をキャンセル。");
        }

        /// <summary>
        /// アプリケーションが再開された場合の処理
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();
            Cts = new CancellationTokenSource();
            Debug.WriteLine($"{DateTime.Now.ToString()} 同期処理を再開。");

            if (!IsSyncRunning)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Debug.WriteLine("同期処理をバックグラウンドで再開しました。");
                        await _syncBackgroundService.StartAsync(Cts.Token);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error: {ex.Message}");
                    }
                });
            }
        }

        #endregion

        #region public Method

        /// <summary>
        /// トークンを渡しているバックグラウンドタスクをキャンセルします。
        /// </summary>
        public static void CancelBackgroundTask()
        {
            Cts.Cancel();
            Cts.Dispose();
        }

        #endregion

    }
}

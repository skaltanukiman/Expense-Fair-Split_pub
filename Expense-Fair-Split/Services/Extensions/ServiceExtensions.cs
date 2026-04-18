using Expense_Fair_Split.Data;
using Expense_Fair_Split.Repositories;
using Expense_Fair_Split.Repositories.Impl;
using Expense_Fair_Split.Services.Api;
using Expense_Fair_Split.Services.Background;
using Expense_Fair_Split.Services.Impl;
using Expense_Fair_Split.Services.Ocr;
using Expense_Fair_Split.Services.RemoteDB;
using Expense_Fair_Split.Services.Sessions;

namespace Expense_Fair_Split.Services.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// サービスコレクションを追加します。
        /// </summary>
        /// <param name="services">サービスコレクション</param>
        /// <returns>追加したサービスコレクション</returns>
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            /*** ローカルDBセットアップ ***/
            services.AddDbContext<AppDbContext>();
            services.AddSingleton<DatabaseInitializer>();

            /*** リモートDBセットアップ ***/
            services.AddSingleton<SyncService>();
            services.AddSingleton<RemoteDbService>();
            services.AddSingleton<SyncBackgroundService>();
            services.AddSingleton<CompareDataService>();
            services.AddSingleton<SyncFlagService>();

            /*** コンフィグ ***/
            services.AddSingleton<ConfigurationService>();

            /*** セッション ***/
            services.AddSingleton<UserSessionService>();
            services.AddSingleton<ViewInputStateService>();

            /*** API ***/
            services.AddSingleton<ApiClient>();

            /*** モデルサービス ***/
            services.AddSingleton<IAlertService, AlertService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccountDataRepository, AccountDataRepository>();
            services.AddScoped<IAccountDataService, AccountDataService>();
            services.AddScoped<IMDistRatioRepository, MDistRatioRepository>();
            services.AddScoped<IMDistRatioService, MDistRatioService>();
            services.AddScoped<IBillingDataRepository, BillingDataRepository>();
            services.AddScoped<IBillingDataService, BillingDataService>();
            services.AddScoped<ISyncDataRepository, SyncDataRepository>();
            services.AddScoped<ISyncDataService, SyncDataService>();
            services.AddScoped<ILogDataRepository, LogDataRepository>();
            services.AddScoped<ILogDataService, LogDataService>();
            services.AddScoped<IMContactContentRepository, MContactContentRepository>();
            services.AddScoped<IMContactContentService, MContactContentService>();

            /*** Androidサービス ***/
#if ANDROID
            services.AddScoped<OcrService_Android, OcrService_Android>();
#endif

            return services;
        }
    }
}

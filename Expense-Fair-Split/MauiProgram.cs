using Expense_Fair_Split.Data;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using System;
using System.Diagnostics;

namespace Expense_Fair_Split
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    // FontAwesomeダウンロードフォント追加
                    fonts.AddFont("fa-Regular-400.otf", "FontAwesomeRegular");
                    fonts.AddFont("fa-Solid-900.otf", "FontAwesomeSolid");

                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureSyncfusionCore();

            // サービスを追加
            builder.Services.AddAppServices();

#if DEBUG
            builder.Logging.AddDebug();
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
#else
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
#endif

            var app = builder.Build();
            using var scope = app.Services.CreateScope();

            // Configurationを設定
            var initializer2 = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
            builder.Configuration.AddConfiguration(initializer2._configuration);

            // configからappsettings.jsonを呼び出し、ライセンスキーを設定
            var license = initializer2._configuration.GetSection("SyncfusionLicense").Value;
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(license);

            // データベース初期化を呼び出し
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            initializer.InitializeDatabase();

            return app;
        }
    }
}

using Microsoft.UI.Xaml;
using Microsoft.Maui.Platform;
using System.Diagnostics;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Views.Error;
using Expense_Fair_Split.Commons;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Expense_Fair_Split.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            var services = Expense_Fair_Split.App.Services;
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var nativeWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (nativeWindow != null)
            {
                nativeWindow.Closed += OnWindowClosed;
            }
            else
            {
                try
                {
                    var logDataService = Expense_Fair_Split.App.Services.GetRequiredService<ILogDataService>();
                    await logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), $"{nameof(nativeWindow)}が取得できませんでした。", null, nameof(OnLaunched), null);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
                finally
                {
                    // nativeWindowが取得できない時点で必ずエラー画面に遷移＆バックグラウンド同期処理をキャンセル
                    Debug.WriteLine($"{DateTime.Now.ToString()} バックグラウンド同期処理をキャンセルします。");
                    Expense_Fair_Split.App.CancelBackgroundTask();
                    Expense_Fair_Split.App.IsSyncRunning = true;  // この時点ではWindowを閉じた時のキャンセル処理が働かないのでエラー画面で再度バックグラウンド処理が走らないようにtrueにしておく

                    Expense_Fair_Split.App.Current!.MainPage = new ErrorPage();
                }
            }
        }

        /// <summary>
        /// WindowUIがクローズした際の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnWindowClosed(object sender, WindowEventArgs args)
        {
            Debug.WriteLine($"{DateTime.Now.ToString()} Windows固有の終了処理を起動します。");
            Expense_Fair_Split.App.CancelBackgroundTask();
            Expense_Fair_Split.App.IsSyncRunning = false;
            Debug.WriteLine($"{DateTime.Now.ToString()} Windows固有の終了処理を終了します。");
        }
    }
}

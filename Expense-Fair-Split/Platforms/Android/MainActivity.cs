using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using AndroidX.AppCompat.App;

namespace Expense_Fair_Split
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        /// <summary>
        /// AndroidMainActivity起動時の処理
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle? savedInstanceState)
        {
#if DEBUG
            Log.Debug("AppLifecycle", "Androidアプリの OnCreate が呼ばれました");
#endif

            // Androidプラットフォームのダークテーマを無効化
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;

            base.OnCreate(savedInstanceState);
        }

        /// <summary>
        /// アプリケーション終了時の処理
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

        #if DEBUG
            Log.Debug("AppLifecycle", "Androidアプリの OnDestroy が呼ばれました");
        #endif

            App.CancelBackgroundTask();
            App.IsSyncRunning = false;
        }
    }
}

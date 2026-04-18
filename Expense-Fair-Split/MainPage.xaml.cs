using Expense_Fair_Split.Views.Contact;
using System.Diagnostics;

namespace Expense_Fair_Split
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnBillingDataEntry(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//BillingDataEntry");
        }

        private async void OnBillingDataInquiry(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//BillingDataInquiry");
        }

        private async void OnMasterRegister(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MasterRegister");
        }

        private async void OnOtherContents(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//OtherContents");
        }

        private async void OnSettingPage(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SettingPage");
        }

        private async void OnContactPage(object sender, TappedEventArgs e)
        {
            if (Application.Current is not null)
            {
                await this.Navigation.PushModalAsync(new NavigationPage(new ContactPage())
                {
#if WINDOWS
                    BarBackgroundColor = (Color)Application.Current.Resources["OffBlack"],
                    BarTextColor = Colors.White
#elif ANDROID
                    BarBackgroundColor = (Color)Application.Current.Resources["Primary"],
                    BarTextColor = Colors.White                 
#endif
                });
            }
            else
            {
                // 問題があった場合はページ遷移せず、そのままラベルを非活性にする
                ContactForm.IsEnabled = false;
            }
        }
    }
}

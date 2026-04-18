using Expense_Fair_Split.Views;
using Expense_Fair_Split.Views.Archive;
using Expense_Fair_Split.Views.Contact;
using Expense_Fair_Split.Views.Error;
using Expense_Fair_Split.Views.ImageRecognition;
using Expense_Fair_Split.Views.Login;
using Expense_Fair_Split.Views.NewRegistrations;

namespace Expense_Fair_Split
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // ルートの登録
            Routing.RegisterRoute(nameof(LoginMenuPage), typeof(LoginMenuPage));
            Routing.RegisterRoute(nameof(NewRegistrationPage), typeof(NewRegistrationPage));
            Routing.RegisterRoute(nameof(BillingDataConfirmPage), typeof(BillingDataConfirmPage));
            Routing.RegisterRoute(nameof(BillingDataDetailPage), typeof(BillingDataDetailPage));
            Routing.RegisterRoute(nameof(AccountEditPage), typeof(AccountEditPage));
            Routing.RegisterRoute(nameof(ErrorPage), typeof(ErrorPage));
            Routing.RegisterRoute(nameof(ContactPage), typeof(ContactPage));
            Routing.RegisterRoute(nameof(BeforeImageRecognitionPage), typeof(BeforeImageRecognitionPage));
            Routing.RegisterRoute(nameof(ArchivePreparationPage), typeof(ArchivePreparationPage));
        }
    }
}

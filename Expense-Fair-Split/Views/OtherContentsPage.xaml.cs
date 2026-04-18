using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Archive;
using System.Diagnostics;

namespace Expense_Fair_Split.Views;

public partial class OtherContentsPage : ContentPage
{
	public OtherContentsPage()
	{
		InitializeComponent();

        this.Loaded += (_, _) =>
        {
            _vm = new OtherContentsViewModel();
            this.BindingContext = _vm;
        };
    }
    OtherContentsViewModel? _vm;

    // ‰æ–Ê‘JˆÚ
    private async void OnArchivePreparationPage(object sender, EventArgs e)
    {
        await this.Navigation.PushAsync(new ArchivePreparationPage());
    }
}
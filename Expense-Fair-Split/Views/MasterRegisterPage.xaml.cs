using Expense_Fair_Split.ViewModels;

namespace Expense_Fair_Split.Views;

public partial class MasterRegisterPage : ContentPage
{
	public MasterRegisterPage()
	{
		InitializeComponent();
		this.Loaded += (_, _) =>
		{
			_vm = new MasterRegisterViewModel();
			this.BindingContext = _vm;
		};
	}
	MasterRegisterViewModel? _vm;

	// ‰æ–Ê‘JˆÚ
    private async void OnAccountEdit(object sender, EventArgs e)
	{
        await this.Navigation.PushAsync(new AccountEditPage());
    }
}
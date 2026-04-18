using Expense_Fair_Split.ViewModels;

namespace Expense_Fair_Split;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{
		InitializeComponent();
		this.Loaded += (_, _) =>
		{
			_vm = new SettingViewModel();
			this.BindingContext = _vm;
		};
	}
	SettingViewModel? _vm;
}
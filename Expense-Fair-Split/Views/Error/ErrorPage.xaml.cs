using Expense_Fair_Split.ViewModels;

namespace Expense_Fair_Split.Views.Error;

public partial class ErrorPage : ContentPage
{
	public ErrorPage(int? errorType = null, int? errorCode = null)
	{
		InitializeComponent();

		this.Loaded += (_, _) =>
		{
			_vm = new ErrorViewModel() { ErrorType = errorType, ErrorCode = errorCode};
			this.BindingContext = _vm;
		};
	}
	ErrorViewModel? _vm;
}
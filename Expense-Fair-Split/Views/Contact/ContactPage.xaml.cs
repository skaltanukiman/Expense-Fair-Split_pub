using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using System.Diagnostics;

namespace Expense_Fair_Split.Views.Contact;

public partial class ContactPage : ContentPage
{
    private readonly IUserService _userService;
    private readonly ILogDataService _logDataService;
    private readonly UserSessionService _userSessionService;

    public ContactPage()
	{
		InitializeComponent();

        var serviceProvider = App.Services;
        _userService = serviceProvider.GetRequiredService<IUserService>();
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();

        this.Loaded += async (_, _) =>
		{
			try
			{
                _vm = new ContactViewModel(this);
                this.BindingContext = _vm;

                await _vm.PickerInitAsync();
            }
			catch (Exception ex)
			{
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, $"{ErrorMsgResource.ConstructorError}|{nameof(ContactPage)}", null);
                Application.Current!.MainPage = new ErrorPage();
            }

			
		};
	}
	ContactViewModel? _vm;

    private async void ReturnHomeAsync(object sender, TappedEventArgs e)
    {
		await this.Navigation.PopModalAsync();
    }
}
using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;

namespace Expense_Fair_Split.Views.Archive;

public partial class ArchiveOutputPage : ContentPage
{
    private readonly ILogDataService _logDataService;
    private readonly UserSessionService _userSessionService;

    public ArchiveOutputPage(ArchiveOutputViewModel viewModel)
	{
		InitializeComponent();

        var serviceProvider = App.Services;
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();

        this.Loaded += async (_, _) =>
		{
			try
			{
				_vm = viewModel;
				this.BindingContext = _vm;

				await _vm.CreateViewDto();
			}
			catch (Exception ex)
			{
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, $"{ErrorMsgResource.ConstructorError}|{nameof(ArchiveOutputPage)}", null);
                Application.Current!.MainPage = new ErrorPage();
            }            
        };
	}
    ArchiveOutputViewModel? _vm;
}
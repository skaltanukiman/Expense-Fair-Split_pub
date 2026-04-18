using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using System.Diagnostics;

namespace Expense_Fair_Split.Views;

public partial class BillingDataDetailPage : ContentPage
{
    private readonly UserSessionService _userSessionService;
    private readonly ILogDataService _logDataService;

    public BillingDataDetailPage(BillingDataDetailViewModel detailData)
	{
		InitializeComponent();
        var serviceProvider = App.Services;
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();

        this.Loaded += async (_, _) =>
		{
			try
			{
                _vm = detailData;

                if (!_vm.SetRelatedPage(this)) throw new InvalidOperationException("ViewModelへの関連ページの紐づけに失敗しました。");
                
                this.BindingContext = _vm;
                _vm.SetDisplayViewStatus();
				await _vm.SetTargetBillingData();
            }
			catch (Exception ex)
			{
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, $"{ErrorMsgResource.ConstructorError}|{nameof(BillingDataDetailPage)}", null);
                Application.Current!.MainPage = new ErrorPage();
            }
        };
	}
	BillingDataDetailViewModel? _vm;
}
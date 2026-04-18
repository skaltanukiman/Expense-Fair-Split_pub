using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.ViewModels;
using Expense_Fair_Split.Views.Error;
using System.Diagnostics;
using static Expense_Fair_Split.ViewModels.ArchivePreparationViewModel;

namespace Expense_Fair_Split.Views.Archive;

public partial class ArchivePreparationPage : ContentPage
{
    private readonly ILogDataService _logDataService;
    private readonly UserSessionService _userSessionService;

    public ArchivePreparationPage()
	{
		InitializeComponent();

        var serviceProvider = App.Services;
        _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
        _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();

        this.Loaded += async (_, _) =>
        {
            try
            {
                _vm = new ArchivePreparationViewModel();
                this.BindingContext = _vm;
                await _vm.InitPickerListAsync();
            }
            catch (Exception ex) 
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, $"{ErrorMsgResource.ConstructorError}|{nameof(ArchivePreparationPage)}", null);
                Application.Current!.MainPage = new ErrorPage();
            }
        };
    }
	ArchivePreparationViewModel? _vm;

    /// <summary>
    /// アーカイブページへの遷移処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ViewModelNotFoundException"></exception>
    /// <exception cref="NotFindItemException"></exception>
    private async void GoToArchivePage(object sender, EventArgs e)
    {
        // 処理中の場合リターン
        if (_vm is null || _vm.CheckProcessStatus(ProcessSelector.Output) is null || _vm.CheckProcessStatus(ProcessSelector.Output) == true) return;

        try
        {
            if (_vm is null) throw new ViewModelNotFoundException(nameof(ArchivePreparationViewModel));
            if (_vm.CachedBillingDataList is null) throw new NotFindItemException($"{nameof(_vm.CachedBillingDataList)} is null");

            _vm.StartProcessing(ProcessSelector.Output);

            ArchiveOutputViewModel archiveVm = new ArchiveOutputViewModel(_vm.CachedBillingDataList);
            archiveVm.SearchKeys.SetFindKeys(_vm.YearPickSelectedItem, _vm.MonthPickSelectedItem, _vm.IncludingDeletedData);

            await this.Navigation.PushAsync(new ArchiveOutputPage(archiveVm));
        }
        catch (Exception ex)
        {
            await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, $"{nameof(ArchivePreparationPage)}", null);
            Application.Current!.MainPage = new ErrorPage();
        }
        finally
        {
            _vm?.EndProcessing(ProcessSelector.Output);
        }
        
    }
}
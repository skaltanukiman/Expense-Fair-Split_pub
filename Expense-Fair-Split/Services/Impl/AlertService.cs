using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Impl
{
    public class AlertService : IAlertService
    {
        public async Task<bool> ShowAlertAsync(string title, string message, string accept, string cancel)
        {
            if (Application.Current?.MainPage is null) throw new InvalidOperationException($"{nameof(MainPage)}が設定されていません。");

            return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }

    }
}

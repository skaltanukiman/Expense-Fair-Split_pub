using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Services.Sessions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Api
{
    public class CommonApiRequests
    {
        private readonly ApiClient _apiClient;
        private readonly ILogDataService _logDataService;
        private readonly UserSessionService _userSessionService;

        public CommonApiRequests(ApiClient apiClient)
        {
            var serviceProvider = App.Services;
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _apiClient = apiClient;
        }

        public async Task<bool> CommonPostAsync<T>(T data, string controllerName)
        {
            try
            {
                HttpResponseMessage response = await _apiClient.PostAsync($"api/{controllerName}", data);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), $"データの登録に失敗しました。('{data?.ToString() ?? string.Empty}',{controllerName})", _userSessionService?.UserId ?? null, nameof(CommonPostAsync), null);
                    return false;
                }
            }
            catch (Exception ex) 
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), ex.Message, _userSessionService?.UserId ?? null, nameof(CommonPostAsync), null);
                return false;            
            }
        }
    }
}

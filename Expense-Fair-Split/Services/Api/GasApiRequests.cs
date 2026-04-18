using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Models.TransportModels;
using Expense_Fair_Split.Services.Sessions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Api
{
    public class GasApiRequests
    {
        private readonly ILogDataService _logDataService;
        private readonly UserSessionService _userSessionService;
        private readonly HttpClient _client;
        private readonly ConfigurationService _configurationService;

        public GasApiRequests()
        {
            var serviceProvider = App.Services;
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _configurationService = serviceProvider.GetRequiredService<ConfigurationService>();
            _client = new HttpClient();
        }

        public async Task<bool> PostContactDataAsync(ContactDataIf contactDataIf)
        {
            try
            {
                string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                string url = _configurationService._configuration.GetSection("ContactContentSheetApiUrl")[environment] ?? string.Empty;

                if (string.IsNullOrWhiteSpace(url))
                {
                    throw new InvalidOperationException($"環境 '{environment}' に対応するURLを取得できませんでした。ContactContentSheetApiUrl が正しく構成されているか確認してください。");
                }

                HttpResponseMessage response = await _client.PostAsJsonAsync(url, contactDataIf);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync();
                    await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), $"失敗: {response.StatusCode},{result}", _userSessionService?.UserId ?? null, nameof(PostContactDataAsync), null);
                    return false;
                }
            }
            catch (Exception ex) 
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), ex.Message, _userSessionService?.UserId ?? null, nameof(PostContactDataAsync), null);
                return false;
            }
        }
    }
}

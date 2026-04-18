using Expense_Fair_Split.Models.TransportModels;
using Expense_Fair_Split.Services.Ocr;
using System.Net.Http.Json;
using static Expense_Fair_Split.Commons.EnumResource;

namespace Expense_Fair_Split.Services.Api
{
    public class ApiClient
    {
        private readonly HttpClient _client = null!;
        private CommonApiRequests _commonApiRequests = null!;
        private GasApiRequests _gasApiRequests = null!;
        private GoogleVisionApiRequests _googleVisionApiRequests = null!;

        public ApiClient()
        {
            var handler = new HttpClientHandler();
            _client = new HttpClient(handler) { BaseAddress = new Uri("") };
        }

        #region API Method

        public async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            return await _client.GetAsync(endpoint);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
        {
            return await _client.PostAsJsonAsync(endpoint, data);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
        {
            return await _client.PutAsJsonAsync(endpoint, data);
        }

        public async Task<HttpResponseMessage> SyncPutAsync<T>(string endpoint, List<T> dataList)
        {
            return await _client.PutAsJsonAsync(endpoint, dataList);
        }

        #endregion

        public void NewApiRequets()
        {
            _commonApiRequests = new CommonApiRequests(this);
            _gasApiRequests = new GasApiRequests();
            _googleVisionApiRequests = new GoogleVisionApiRequests();
        }

        public async Task<bool> CommonHttpRequetsAsync<T>(T data, string controllerName, HTTPKey key)
        {
            switch (key)
            {
                case HTTPKey.Post:
                    return await _commonApiRequests.CommonPostAsync(data, controllerName);
                default:
                    return false;
            }
        }

        public async Task<bool> GasApiFuncExec(ContactDataIf contactDataIf)
        {
            return await _gasApiRequests.PostContactDataAsync(contactDataIf);
        }

        public async Task<(bool, PostVisionDto)> GoogleVisionApiExec(VisionRequest visionRequest, GoogleVisionApiRequests.ParseMode parseMode)
        {
            return await _googleVisionApiRequests.PostVisionRequestAsync(visionRequest, parseMode);
        }
    }
}

using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Repositories;
using Expense_Fair_Split.Services.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Impl
{
    public class LogDataService : ILogDataService
    {
        private readonly ILogDataRepository _logDataRepository;
        private readonly AppDbContext _context;
        private readonly ApiClient _apiClient;

        public LogDataService(ILogDataRepository logDataRepository, AppDbContext context, ApiClient apiClient)
        {
            _logDataRepository = logDataRepository;
            _context = context;
            _apiClient = apiClient;
        }

        public async Task<LogData?> GetLogDataAsync(int id)
        {
            return await _logDataRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<LogData>> GetAllLogDataAsync()
        {
            return await _logDataRepository.GetAllAsync();
        }

        public async Task CreateLogDataAsync(LogData logData)
        {
            try
            {
                await _logDataRepository.AddAsync(logData);
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task UpdateLogDataAsync(LogData logData)
        {
            try
            {
                await _logDataRepository.UpdateAsync(logData);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);
                _context.ChangeTracker.Clear();
                throw;
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task DeleteLogDataAsync(int id)
        {
            await _logDataRepository.DeleteAsync(id);
        }

        public async Task InsertLog(string logLevel, string? message = null, int? userId = null, string? source = null, object? extraData = null)
        {
            try
            {
                string? jsonExtra = null;
                if (extraData is not null) jsonExtra = JsonSerializer.Serialize(extraData);  // jsonオブジェクトが渡されている場合のみ、文字列に変換してjsonExtraに格納する。

                // SQLite（ローカルDB）に登録
                DateTime jstTime = CommonUtil.CreateTokyoJapanCurrentDateTime();
                LogData logData = new LogData
                {
                    Timestamp = jstTime,
                    LogLevel = logLevel,
                    Message = message,
                    UserId = userId,
                    Source = source,
                    ExtraData = jsonExtra
                };

                await CreateLogDataAsync(logData);

                // PostgreSQL（サーバーDB）に登録
                var response = await _apiClient.PostAsync("api/LogData", logData);

                if (response.IsSuccessStatusCode)
                {
                    logData.IsSynced = true;  // PostgreSQLへの登録成功時は同期フラグを同期済みに変更
                    await UpdateLogDataAsync(logData);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex) 
            {
                // ログの登録に失敗した場合の処理       
                Debug.WriteLine(ex.Message);
            }
        }
    }
}

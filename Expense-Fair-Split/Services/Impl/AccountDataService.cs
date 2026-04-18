using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Expense_Fair_Split.Services.Impl
{
    public class AccountDataService : IAccountDataService
    {
        private readonly IAccountDataRepository _accountDataRepository;
        private readonly AppDbContext _context;

        public AccountDataService(IAccountDataRepository accountDataRepository, AppDbContext context)
        {
            _accountDataRepository = accountDataRepository;
            _context = context;
        }

        public async Task<AccountData?> GetAccountDataAsync(int id)
        {
            return await _accountDataRepository.GetByIdAsync(id);
        }

        public async Task<AccountData?> GetAccountDataByAccountNameAsync(string accountName)
        {
            return await _accountDataRepository.GetByAccountNameAsync(accountName);
        }

        public async Task<IEnumerable<AccountData>> GetAllAccountDataAsync()
        {
            return await _accountDataRepository.GetAllAsync();
        }

        public async Task CreateAccountDataAsync(AccountData accountData)
        {
            try
            {
                await _accountDataRepository.AddAsync(accountData);
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task UpdateAccountDataAsync(AccountData accountData)
        {
            try
            {
                await _accountDataRepository.UpdateAsync(accountData);
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

        public async Task DeleteAccountDataAsync(int id)
        {
            await _accountDataRepository.DeleteAsync(id);
        }
    }
}

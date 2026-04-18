using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense_Fair_Split.Repositories.Impl
{
    public class AccountDataRepository : IAccountDataRepository
    {
        private readonly AppDbContext _context;

        public AccountDataRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AccountData?> GetByIdAsync(int id)
        {
            return await _context.AccountDataSet.FindAsync(id);
        }

        public async Task<AccountData?> GetByAccountNameAsync(string accountName)
        {
            AccountData? accountData = await _context.AccountDataSet.FirstOrDefaultAsync(acc => acc.AccName == accountName);
            if (accountData is null)
            {
                return null;
            }

            return accountData;
        }

        public async Task<IEnumerable<AccountData>> GetAllAsync()
        {
            return await _context.AccountDataSet.ToListAsync();
        }

        public async Task AddAsync(AccountData accountData)
        {
            _context.AccountDataSet.Add(accountData);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AccountData accountData)
        {
            _context.AccountDataSet.Update(accountData);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            AccountData? accountData = await GetByIdAsync(id);
            if (accountData != null)
            {
                _context.AccountDataSet.Remove(accountData);
                await _context.SaveChangesAsync();
            }
        }
    }
}

using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Repositories
{
    public interface IAccountDataRepository
    {
        Task<AccountData?> GetByIdAsync(int id);
        Task<AccountData?> GetByAccountNameAsync(string accountName);
        Task<IEnumerable<AccountData>> GetAllAsync();
        Task AddAsync(AccountData accountData);
        Task UpdateAsync(AccountData accountData);
        Task DeleteAsync(int id);

    }
}

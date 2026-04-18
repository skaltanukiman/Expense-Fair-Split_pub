using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Repositories
{
    public interface IBillingDataRepository
    {
        Task<BillingData?> GetByIdAsync(int id);
        Task<IEnumerable<BillingData>> GetAllByFromUserCodeFindAsync(int userCode);
        Task<IEnumerable<BillingData>> GetAllByToUserCodeFindAsync(int userCode);
        Task<IEnumerable<BillingData>> GetAllAsync();
        Task AddAsync(BillingData billingData);
        Task UpdateAsync(BillingData billingData);
        Task DeleteAsync(int id);
    }
}

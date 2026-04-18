using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Repositories
{
    public interface ILogDataRepository
    {
        Task<LogData?> GetByIdAsync(int id);
        Task<IEnumerable<LogData>> GetAllAsync();
        Task AddAsync(LogData logData);
        Task UpdateAsync(LogData logData);
        Task DeleteAsync(int id);
    }
}

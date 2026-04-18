using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Repositories
{
    public interface IMContactContentRepository
    {
        Task<MContactContent?> GetByIdAsync(int id);
        Task<IEnumerable<MContactContent>> GetAllByContactTypeFindAsync(string contactType);
        Task<IEnumerable<MContactContent>> GetAllAsync();
    }
}

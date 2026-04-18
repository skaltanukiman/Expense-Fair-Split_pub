using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Repositories
{
    public interface ISyncDataRepository
    {
        Task UpdateAsync<T>(T data) where T : class;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services
{
    public interface ISyncDataService
    {
        Task UpdateDataAsync<T>(T data) where T : class;
    }
}

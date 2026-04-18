using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Repositories.Impl
{
    public class SyncDataRepository : ISyncDataRepository
    {
        private readonly AppDbContext _context;

        public SyncDataRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateAsync<T>(T data) where T : class
        {
            _context.Set<T>().Update(data);
            await _context.SaveChangesAsync();
        }

    }
}

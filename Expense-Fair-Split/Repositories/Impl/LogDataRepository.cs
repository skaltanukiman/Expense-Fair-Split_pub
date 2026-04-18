using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Repositories.Impl
{
    public class LogDataRepository : ILogDataRepository
    {
        private readonly AppDbContext _context;

        public LogDataRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LogData?> GetByIdAsync(int id)
        {
            return await _context.LogDataSet.FindAsync(id);
        }

        public async Task<IEnumerable<LogData>> GetAllAsync()
        {
            return await _context.LogDataSet.ToListAsync();
        }

        public async Task AddAsync(LogData logData)
        {
            _context.LogDataSet.Add(logData);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LogData logData)
        {
            _context.LogDataSet.Update(logData);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            LogData? logData = await GetByIdAsync(id);
            if (logData != null)
            {
                _context.LogDataSet.Remove(logData);
                await _context.SaveChangesAsync();
            }
        }
    }
}

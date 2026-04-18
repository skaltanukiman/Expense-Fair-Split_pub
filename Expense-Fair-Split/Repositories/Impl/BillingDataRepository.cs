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
    public class BillingDataRepository : IBillingDataRepository
    {
        private readonly AppDbContext _context;

        public BillingDataRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<BillingData?> GetByIdAsync(int id)
        {
            return await _context.BillingDataSet.FindAsync(id);
        }

        public async Task<IEnumerable<BillingData>> GetAllByFromUserCodeFindAsync(int userCode)
        {
            return await _context.BillingDataSet.Where(data => data.FromUserCode == userCode).ToListAsync();
        }

        public async Task<IEnumerable<BillingData>> GetAllByToUserCodeFindAsync(int userCode)
        {
            return await _context.BillingDataSet.Where(data => data.ToUserCode == userCode).ToListAsync();
        }

        public async Task<IEnumerable<BillingData>> GetAllAsync()
        {
            return await _context.BillingDataSet.ToListAsync();
        }

        public async Task AddAsync(BillingData billingData)
        {
            _context.BillingDataSet.Add(billingData);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BillingData billingData)
        {
            _context.BillingDataSet.Update(billingData);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            BillingData? billingData = await GetByIdAsync(id);
            if (billingData != null)
            {
                _context.BillingDataSet.Remove(billingData);
                await _context.SaveChangesAsync();
            }
        }
    }
}

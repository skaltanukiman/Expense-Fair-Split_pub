using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Impl
{
    public class BillingDataService : IBillingDataService
    {
        private readonly IBillingDataRepository _billingDataRepository;
        private readonly AppDbContext _context;

        public BillingDataService(IBillingDataRepository billingDataRepository, AppDbContext context) 
        {
            _billingDataRepository = billingDataRepository;
            _context = context;
        }

        public async Task<BillingData?> GetBillingDataAsync(int id)
        {
            return await _billingDataRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<BillingData>> GetAllByFromUserCodeFindAsync(int userCode)
        {
            return await _billingDataRepository.GetAllByFromUserCodeFindAsync(userCode);
        }

        public async Task<IEnumerable<BillingData>> GetAllByToUserCodeFindAsync(int userCode)
        {
            return await _billingDataRepository.GetAllByToUserCodeFindAsync(userCode);
        }

        public async Task<IEnumerable<BillingData>> GetAllBillingDataAsync()
        {
            return await _billingDataRepository.GetAllAsync();
        }

        public async Task CreateBillingDataAsync(BillingData billingData)
        {
            try
            {
                await _billingDataRepository.AddAsync(billingData);
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task UpdateBillingDataAsync(BillingData billingData)
        {
            try
            {
                await _billingDataRepository.UpdateAsync(billingData);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);
                _context.ChangeTracker.Clear();
                throw;
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task DeleteBillingDataAsync(int id)
        {
            await _billingDataRepository.DeleteAsync(id);
        }
    }
}

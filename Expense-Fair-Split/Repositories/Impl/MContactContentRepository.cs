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
    public class MContactContentRepository : IMContactContentRepository
    {
        private readonly AppDbContext _context;

        public MContactContentRepository(AppDbContext context)
        {
            _context = context;
        }

        #region Read

        public async Task<MContactContent?> GetByIdAsync(int id)
        {
            return await _context.MContactContentSet.FindAsync(id);
        }

        public async Task<IEnumerable<MContactContent>> GetAllByContactTypeFindAsync(string contactType)
        {
            return await _context.MContactContentSet.Where(m => m.ContactType == contactType).ToListAsync();
        }

        public async Task<IEnumerable<MContactContent>> GetAllAsync()
        {
            return await _context.MContactContentSet.ToListAsync();
        }

        #endregion
    }
}

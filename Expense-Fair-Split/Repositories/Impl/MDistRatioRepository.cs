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
    public class MDistRatioRepository : IMDistRatioRepository
    {
        private readonly AppDbContext _context;

        public MDistRatioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MDistRatio?> GetByIdAsync(int typeCode, int code)
        {
            return await _context.MDistRatioSet.FindAsync(typeCode, code);
        }

        public async Task<IEnumerable<MDistRatio>> GetAllByRatioTypeCodeFindAsync(int typeCode)
        {
            return await _context.MDistRatioSet.Where(m => m.RatioTypeCode == typeCode).ToListAsync();
        }

        public async Task<IEnumerable<MDistRatio>> GetAllAsync()
        {
            return await _context.MDistRatioSet.ToListAsync();
        }

        public async Task AddAsync(MDistRatio distRatio)
        {
            _context.MDistRatioSet.Add(distRatio);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MDistRatio distRatio)
        {
            _context.MDistRatioSet.Update(distRatio);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int typeCode, int code)
        {
            MDistRatio? distRatio = await GetByIdAsync(typeCode, code);
            if (distRatio != null)
            {
                _context.MDistRatioSet.Remove(distRatio);
                await _context.SaveChangesAsync();
            }
        }
    }
}

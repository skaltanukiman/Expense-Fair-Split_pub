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
    public class MDistRatioService : IMDistRatioService
    {
        private readonly IMDistRatioRepository _distRatioRepository;
        private readonly AppDbContext _context;

        public MDistRatioService(IMDistRatioRepository distRatioRepository, AppDbContext context)
        {
            _distRatioRepository = distRatioRepository;
            _context = context;
        }

        public async Task<MDistRatio?> GetByKeyMDistRatioAsync(int typeCode, int code)
        {
            return await _distRatioRepository.GetByIdAsync(typeCode, code);
        }

        public async Task<IEnumerable<MDistRatio>> GetAllByRatioTypeCodeFindAsync(int typeCode)
        {
            return await _distRatioRepository.GetAllByRatioTypeCodeFindAsync(typeCode);
        }

        public async Task<IEnumerable<MDistRatio>> GetAllMDistRatioAsync()
        {
            return await _distRatioRepository.GetAllAsync();
        }

        public async Task CreateMDistRatioAsync(MDistRatio distRatio)
        {
            try
            {
                await _distRatioRepository.AddAsync(distRatio);
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task UpdateMDistRatioAsync(MDistRatio distRatio)
        {
            try
            {
                await _distRatioRepository.UpdateAsync(distRatio);
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

        public async Task DeleteMDistRatioAsync(int typeCode, int code)
        {
            await _distRatioRepository.DeleteAsync(typeCode, code);
        }
    }
}

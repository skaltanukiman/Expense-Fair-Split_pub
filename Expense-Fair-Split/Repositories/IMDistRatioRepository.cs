using Expense_Fair_Split.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Repositories
{
    public interface IMDistRatioRepository
    {
        Task<MDistRatio?> GetByIdAsync(int typeCode, int code);
        Task<IEnumerable<MDistRatio>> GetAllByRatioTypeCodeFindAsync(int typeCode);
        Task<IEnumerable<MDistRatio>> GetAllAsync();
        Task AddAsync(MDistRatio distRatio);
        Task UpdateAsync(MDistRatio distRatio);
        Task DeleteAsync(int typeCode, int code);
    }
}

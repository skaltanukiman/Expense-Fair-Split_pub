using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Expense_Fair_Split.Commons.EnumResource;

namespace Expense_Fair_Split.Services.Impl
{
    public class MContactContentService : IMContactContentService
    {
        private readonly IMContactContentRepository _contactContentRepository;
        private readonly AppDbContext _context;

        public MContactContentService(IMContactContentRepository contactContentRepository, AppDbContext context)
        {
            _contactContentRepository = contactContentRepository;
            _context = context;
        }

        #region Read

        public async Task<MContactContent?> GetByKeyMContactContentAsync(int id)
        {
            return await _contactContentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<MContactContent>> GetAllByContactTypeFindAsync(string contactType, OrderKey orderKey)
        {
            IEnumerable<MContactContent> dataSet = await _contactContentRepository.GetAllByContactTypeFindAsync(contactType);

            if (!dataSet.Any()) return dataSet;  // データが見つからない場合はそのまま返す

            // データが存在する場合は、並べ替えを行い返す
            switch (orderKey)
            {
                case OrderKey.Asc:
                    dataSet = dataSet.OrderBy(data => data.SelectNum);
                    break;
                case OrderKey.Desc:
                    dataSet = dataSet.OrderByDescending(data => data.SelectNum);
                    break;
                default:
                    dataSet = dataSet.OrderBy(data => data.SelectNum);  // キーが範囲外の場合は昇順で返す
                    break;
            }
            
            return dataSet.ToList();
        }

        public async Task<IEnumerable<MContactContent>> GetAllMContactContentAsync()
        {
            return await _contactContentRepository.GetAllAsync();
        }

        #endregion
    }
}

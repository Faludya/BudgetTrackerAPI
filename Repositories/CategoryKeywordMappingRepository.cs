using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class CategoryKeywordMappingRepository : RepositoryBase<CategoryKeywordMapping>, ICategoryKeywordMappingRepository
    {
        public CategoryKeywordMappingRepository(AppDbContext context) : base(context) { }

        public async Task<CategoryKeywordMapping?> GetByIdAsync(int id)
        {
            return await _appDbContext.CategoryKeywordMappings
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<CategoryKeywordMapping?> GetByKeywordAsync(string userId, string keyword)
        {
            return await _appDbContext.CategoryKeywordMappings
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.UserId == userId && m.Keyword == keyword.ToLower());
        }

        public async Task<List<CategoryKeywordMapping>> GetAllForUserAsync(string userId)
        {
            return await _appDbContext.CategoryKeywordMappings
                .Where(m => m.UserId == userId)
                .Include(m => m.Category)
                .ToListAsync();
        }
    }

}

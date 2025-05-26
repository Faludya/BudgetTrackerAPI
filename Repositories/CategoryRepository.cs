using System;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<Category> GetCategoryById(int id)
        {
            return await _appDbContext.Categories.FindAsync(id);
        }

        public async Task<Category> GetCategoryByUserIdAndName(string userId, string cateogryName)
        {
            return await _appDbContext.Categories.Where(c => c.UserId == userId && c.Name.ToLower() == cateogryName.ToLower()).FirstAsync();
        }
    }
}

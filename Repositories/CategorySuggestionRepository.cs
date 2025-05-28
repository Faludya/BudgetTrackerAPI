using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class CategorySuggestionRepository : RepositoryBase<CategorySuggestion>, ICategorySuggestionRepository
    {
        public CategorySuggestionRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<CategorySuggestion?> FindSuggestionByKeywordAsync(string keyword, string userId)
        {
            return await _appDbContext.CategorySuggestions
                .Include(cs => cs.Category)
                .Where(cs => cs.SourceKeyword == keyword.ToLower() && !cs.IsFromMLModel && cs.Category.UserId == userId)
                .OrderByDescending(cs => cs.Confidence)
                .FirstOrDefaultAsync();
        }

        public async Task<CategorySuggestion?> GetCategorySuggestionById(int id)
        {
            return await _appDbContext.CategorySuggestions
                .Include(cs => cs.Category)
                .Where(cs => cs.Id == id)
                .OrderByDescending(cs => cs.Confidence)
                .FirstOrDefaultAsync();
        }
    }
}

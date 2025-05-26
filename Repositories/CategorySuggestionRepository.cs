using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class CategorySuggestionRepository : RepositoryBase<CategorySuggestion>, ICategorySuggestionRepository
    {
        public CategorySuggestionRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

    }
}

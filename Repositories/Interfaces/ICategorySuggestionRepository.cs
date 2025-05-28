using Models;

namespace Repositories.Interfaces
{
    public interface ICategorySuggestionRepository : IRepositoryBase<CategorySuggestion>
    {

        Task<CategorySuggestion?> GetCategorySuggestionById(int id);
        Task<CategorySuggestion?> FindSuggestionByKeywordAsync(string keyword, string userId);
    }
}

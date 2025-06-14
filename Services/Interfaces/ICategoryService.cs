using Models;
using Models.DTOs;

namespace Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync(string userId);
        Task<Category> GetCategoryByIdAsync(int id);
        Task ReorderCategoriesAsync(List<CategoryReorderDto> reorderedCategories);
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
        Task CreateDefaultCategories(string userId);
        Task<string?> PredictCategoryAsync(string description);
    }
}

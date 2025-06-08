using Models.DTOs;
using Models;

namespace Services.Interfaces
{
    public interface ICategoryKeywordMappingService
    {
        Task<List<CategoryKeywordMapping>> GetAllForUserAsync(string userId);
        Task<CategoryKeywordMapping?> GetByIdAsync(int id);
        Task AddAsync(string userId, CategoryKeywordMappingDto dto);
        Task<bool> UpdateAsync(string userId, UpdateCategoryKeywordMappingDto dto);
        Task<bool> DeleteAsync(string userId, int id);
        Task<int?> FindCategoryIdByKeywordAsync(string userId, string description);
    }

}

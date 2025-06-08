using Models;

namespace Repositories.Interfaces
{
    public interface ICategoryKeywordMappingRepository : IRepositoryBase<CategoryKeywordMapping>
    {
        Task<CategoryKeywordMapping?> GetByIdAsync(int id);
        Task<CategoryKeywordMapping?> GetByKeywordAsync(string userId, string keyword);
        Task<List<CategoryKeywordMapping>> GetAllForUserAsync(string userId);
    }

}

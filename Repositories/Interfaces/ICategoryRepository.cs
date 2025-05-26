using Models;

namespace Repositories.Interfaces
{
    public interface ICategoryRepository : IRepositoryBase<Category>
    {
        Task<Category> GetCategoryById(int id);
        Task<Category> GetCategoryByUserIdAndName(string userId, string cateogryName);
    }
}

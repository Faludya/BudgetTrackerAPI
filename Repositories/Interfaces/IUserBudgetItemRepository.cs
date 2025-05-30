using Models;

namespace Repositories.Interfaces
{
    public interface IUserBudgetItemRepository : IRepositoryBase<UserBudgetItem>
    {
        Task<UserBudgetItem> GetUserBudgetItemById(int id);
    }
}

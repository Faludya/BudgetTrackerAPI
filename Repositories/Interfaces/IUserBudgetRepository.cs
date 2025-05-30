using Models;

namespace Repositories.Interfaces
{
    public interface IUserBudgetRepository : IRepositoryBase<UserBudget>
    {
        Task<UserBudget> GetUserBudgetById(int id);
    }
}

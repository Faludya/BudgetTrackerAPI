using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class UserBudgetRepository : RepositoryBase<UserBudget>, IUserBudgetRepository
    {
        public UserBudgetRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<UserBudget> GetUserBudgetById(int id)
        {
            return await _appDbContext.UserBudgets.FindAsync(id);
        }
    }
}

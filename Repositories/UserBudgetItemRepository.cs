using Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class UserBudgetItemRepository : RepositoryBase<UserBudgetItem>, IUserBudgetItemRepository
    {
        public UserBudgetItemRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<UserBudgetItem> GetUserBudgetItemById(int id)
        {
            return await _appDbContext.UserBudgetItems.FindAsync(id);
        }
    }
}

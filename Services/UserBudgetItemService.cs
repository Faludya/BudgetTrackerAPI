using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class UserBudgetItemService : IUserBudgetItemService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UserBudgetItemService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task AddItemAsync(UserBudgetItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Item cannot be null.");
            }

            await _repositoryWrapper.UserBudgetItemRepository.Create(item);
            await _repositoryWrapper.Save();
        }

        public async Task DeleteItemAsync(int id)
        {
            var query = await _repositoryWrapper.UserBudgetItemRepository
                .FindByCondition(i => i.Id == id);
            var item = await query.FirstOrDefaultAsync();

            await _repositoryWrapper.UserBudgetItemRepository.Delete(item);
            await _repositoryWrapper.Save();
        }

        public async Task<IEnumerable<UserBudgetItem>> GetItemsByBudgetIdAsync(int budgetId)
        {
            var query = await _repositoryWrapper.UserBudgetItemRepository
                .FindByCondition(i => i.UserBudgetId == budgetId);
            var items = await query
                .Include(i => i.Category)
                .ToListAsync();

            return items;
        }

        public async Task UpdateItemAsync(UserBudgetItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Item cannot be null.");
            }

            await _repositoryWrapper.UserBudgetItemRepository.Update(item);
            await _repositoryWrapper.Save();
        }
    }

}

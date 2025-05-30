using Models;

namespace Services.Interfaces
{
    public interface IUserBudgetItemService
    {
        Task<IEnumerable<UserBudgetItem>> GetItemsByBudgetIdAsync(int budgetId);
        Task AddItemAsync(UserBudgetItem item);
        Task UpdateItemAsync(UserBudgetItem item);
        Task DeleteItemAsync(int id);
    }

}

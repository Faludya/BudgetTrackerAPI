using Models;

namespace Services.Interfaces
{
    public interface IUserBudgetService
    {
        Task<IEnumerable<UserBudget>> GetUserBudgetsAsync(string userId);
        Task<UserBudget?> GetBudgetForMonthAsync(string userId, int month, int year);
        Task AddBudgetAsync(UserBudget budget);
        Task UpdateBudgetAsync(UserBudget budget);
        Task DeleteBudgetAsync(int id);
    }

}

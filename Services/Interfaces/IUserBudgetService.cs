using Models;
using Models.DTOs;

namespace Services.Interfaces
{
    public interface IUserBudgetService
    {
        Task<IEnumerable<UserBudget>> GetUserBudgetsAsync(string userId);
        Task<UserBudget?> GetBudgetForMonthAsync(string userId, int month, int year);
        Task<UserBudget> CreateBudgetFromTemplateAsync(string userId, int month, int year, int templateId, decimal income);
        Task<UserBudgetItem?> AddOrUpdateCategoryLimitAsync(CategoryLimitDto categoryLimitDto);

        Task AddBudgetAsync(UserBudget budget);
        Task UpdateBudgetAsync(UserBudget budget);
        Task DeleteBudgetAsync(int id);
    }

}

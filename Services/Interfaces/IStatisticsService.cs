using Models.DTOs;

namespace Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<SummaryDto> GetSummaryAsync(string userId, int month, int year);
        Task<IEnumerable<TopCategoryDto>> GetTopCategoriesAsync(string userId, int month, int year);
        Task<IEnumerable<MonthlyExpenseDto>> GetMonthlyExpensesAsync(string userId);
        Task<BudgetVsActualDto> GetBudgetVsActualAsync(string userId, int month, int year);
        Task<List<CategoryBudgetUsageDto>> GetBudgetUsageByCategoryAsync(string userId, int month, int year);
        Task<List<string>> GetOverspentCategoryNamesAsync(string userId, int month, int year);
        Task<BudgetHealthSummaryDto> GetBudgetHealthSummaryAsync(string userId, int month, int year);
        Task<List<CurrencyHistoryDto>> GetCurrencyHistoryAsync(string userId, int days);


    }
}

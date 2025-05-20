using Models.DTOs;

namespace Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<SummaryDto> GetSummaryAsync(string userId, int month, int year);
        Task<IEnumerable<TopCategoryDto>> GetTopCategoriesAsync(string userId, int month, int year);
        Task<IEnumerable<MonthlyExpenseDto>> GetMonthlyExpensesAsync(string userId);

    }
}

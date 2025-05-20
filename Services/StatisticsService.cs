using Models.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public StatisticsService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<SummaryDto> GetSummaryAsync(string userId, int month, int year)
        {
            var transactions = await _repositoryWrapper.TransactionRepository.GetTransactions(userId);
            var filtered = transactions
                .Where(t => t.Date.Month == month && t.Date.Year == year);

            var totalIncome = filtered
                .Where(t => t.Type == "Credit")
                .Sum(t => t.Amount);

            var totalExpense = filtered
                .Where(t => t.Type == "Debit")
                .Sum(t => t.Amount);

            return new SummaryDto
            {
                TotalIncome = Math.Round(totalIncome, 2),
                TotalExpense = Math.Round(totalExpense, 2)
            };
        }

        public async Task<IEnumerable<TopCategoryDto>> GetTopCategoriesAsync(string userId, int month, int year)
        {
            var transactions = await _repositoryWrapper.TransactionRepository.GetTransactions(userId);

            var filtered = transactions
                .Where(t => t.Type == "Debit" && t.Date.Month == month && t.Date.Year == year && t.Category != null);

            var grouped = filtered
                .GroupBy(t => t.Category.Name)
                .Select(g => new TopCategoryDto
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(g => g.Amount)
                .Take(5)
                .ToList();

            var total = grouped.Sum(g => g.Amount);

            foreach (var g in grouped)
            {
                g.Percentage = (double)(total == 0 ? 0 : Math.Round((g.Amount / total) * 100, 1));
            }

            return grouped;
        }

        public async Task<IEnumerable<MonthlyExpenseDto>> GetMonthlyExpensesAsync(string userId)
        {
            var transactions = await _repositoryWrapper.TransactionRepository.GetTransactions(userId);

            var grouped = transactions
                .Where(t => t.Type == "Debit")
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyExpenseDto
                {
                    Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM}",
                    Amount = g.Sum(t => t.Amount)
                });

            return grouped.ToList();
        }
    }
}

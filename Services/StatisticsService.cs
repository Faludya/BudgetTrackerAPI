using Microsoft.EntityFrameworkCore;
using Models.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IUserPreferencesService _userPreferencesService;

        public StatisticsService(
            IRepositoryWrapper repositoryWrapper,
            IUserPreferencesService userPreferencesService)
        {
            _repositoryWrapper = repositoryWrapper;
            _userPreferencesService = userPreferencesService;
        }

        public async Task<SummaryDto> GetSummaryAsync(string userId, int month, int year)
        {
            var preferences = await _userPreferencesService.GetUserPreferencesByIdAsync(userId);
            var transactions = await _repositoryWrapper.TransactionRepository.GetTransactions(userId);

            var filtered = transactions
                .Where(t => t.Date.Month == month && t.Date.Year == year);

            var totalIncome = await SumConverted(filtered.Where(t => t.Type == "Credit"), preferences.PreferredCurrency);
            var totalExpense = await SumConverted(filtered.Where(t => t.Type == "Debit"), preferences.PreferredCurrency);

            return new SummaryDto
            {
                TotalIncome = Math.Round(totalIncome, 2),
                TotalExpense = Math.Round(totalExpense, 2)
            };
        }

        public async Task<IEnumerable<TopCategoryDto>> GetTopCategoriesAsync(string userId, int month, int year)
        {
            var preferences = await _userPreferencesService.GetUserPreferencesByIdAsync(userId);
            var transactions = await _repositoryWrapper.TransactionRepository.GetTransactions(userId);

            var filtered = transactions
                .Where(t => t.Type == "Debit" && t.Date.Month == month && t.Date.Year == year && t.Category != null);

            var grouped = new List<TopCategoryDto>();

            foreach (var group in filtered.GroupBy(t => t.Category.Name))
            {
                var sum = await SumConverted(group, preferences.PreferredCurrency);
                grouped.Add(new TopCategoryDto
                {
                    Category = group.Key,
                    Amount = sum
                });
            }

            grouped = grouped.OrderByDescending(g => g.Amount).Take(5).ToList();
            var total = grouped.Sum(g => g.Amount);

            foreach (var g in grouped)
            {
                g.Percentage = (double)(total == 0 ? 0 : Math.Round((g.Amount / total) * 100, 1));
            }

            return grouped;
        }

        public async Task<IEnumerable<MonthlyExpenseDto>> GetMonthlyExpensesAsync(string userId)
        {
            var preferences = await _userPreferencesService.GetUserPreferencesByIdAsync(userId);
            var transactions = await _repositoryWrapper.TransactionRepository.GetTransactions(userId);

            var grouped = transactions
                .Where(t => t.Type == "Debit")
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month);

            var result = new List<MonthlyExpenseDto>();

            foreach (var g in grouped)
            {
                var amount = await SumConverted(g, preferences.PreferredCurrency);
                result.Add(new MonthlyExpenseDto
                {
                    Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM}",
                    Amount = amount
                });
            }

            return result;
        }

        public async Task<BudgetVsActualDto> GetBudgetVsActualAsync(string userId, int month, int year)
        {
            var preferences = await _userPreferencesService.GetUserPreferencesByIdAsync(userId);
            var monthDate = new DateTime(year, month, 1);

            var budgets = await _repositoryWrapper.UserBudgetRepository
                .FindByCondition(b => b.UserId == userId && b.Month.Year == year && b.Month.Month == month);

            var budget = await budgets.Include(b => b.BudgetItems).FirstOrDefaultAsync();

            decimal totalBudgeted = 0;
            if (budget != null)
            {
                foreach (var item in budget.BudgetItems)
                {
                    var budgetCurrency = await _repositoryWrapper.CurrencyRepository.GetCurrencyByCode(budget.CurrencyCode);
                    var preferedCurrency = await _repositoryWrapper.CurrencyRepository.GetCurrencyByCode(preferences.PreferredCurrency);
                    totalBudgeted += CurrencyHelper.ConvertAmount(item.Limit, budgetCurrency, preferedCurrency);
                }
            }

            var transactions = await _repositoryWrapper.TransactionRepository
                .FindByCondition(t => t.UserId == userId &&
                                      t.Date.Month == month &&
                                      t.Date.Year == year &&
                                      t.Type == "Expense");

            var totalSpent = await SumConverted(await transactions.ToListAsync(), preferences.PreferredCurrency);

            return new BudgetVsActualDto
            {
                TotalBudgeted = totalBudgeted,
                TotalSpent = totalSpent
            };
        }

        public async Task<List<CategoryBudgetUsageDto>> GetBudgetUsageByCategoryAsync(string userId, int month, int year)
        {
            var preferences = await _userPreferencesService.GetUserPreferencesByIdAsync(userId);
            var monthDate = new DateTime(year, month, 1);

            var budgets = await _repositoryWrapper.UserBudgetRepository
                .FindByCondition(b => b.UserId == userId && b.Month.Year == year && b.Month.Month == month);

            var budget = await budgets
                .Include(b => b.BudgetItems)
                    .ThenInclude(bi => bi.Category)
                .FirstOrDefaultAsync();

            if (budget == null) return new List<CategoryBudgetUsageDto>();

            var transactions = await _repositoryWrapper.TransactionRepository
                .FindByCondition(t => t.UserId == userId &&
                                      t.Date.Month == month &&
                                      t.Date.Year == year &&
                                      t.Type == "Expense");

            var transactionList = await transactions.ToListAsync();

            var result = new List<CategoryBudgetUsageDto>();

            foreach (var item in budget.BudgetItems.Where(bi => bi.CategoryId.HasValue && bi.Category != null))
            {
                var spent = await SumConverted(
                    transactionList.Where(t => t.CategoryId == item.CategoryId),
                    preferences.PreferredCurrency);

                var budgetCurrency = await _repositoryWrapper.CurrencyRepository.GetCurrencyByCode(budget.CurrencyCode);
                var preferedCurrency = await _repositoryWrapper.CurrencyRepository.GetCurrencyByCode(preferences.PreferredCurrency);
                var limit = CurrencyHelper.ConvertAmount(item.Limit, budgetCurrency, preferedCurrency);

                result.Add(new CategoryBudgetUsageDto
                {
                    CategoryId = item.CategoryId,
                    CategoryName = item.Category!.Name,
                    Limit = limit,
                    Spent = spent
                });
            }

            return result;
        }

        public async Task<List<string>> GetOverspentCategoryNamesAsync(string userId, int month, int year)
        {
            var usageList = await GetBudgetUsageByCategoryAsync(userId, month, year);
            return usageList
                .Where(u => u.IsOverLimit)
                .Select(u => u.CategoryName)
                .ToList();
        }

        public async Task<BudgetHealthSummaryDto> GetBudgetHealthSummaryAsync(string userId, int month, int year)
        {
            var usageList = await GetBudgetUsageByCategoryAsync(userId, month, year);

            var total = usageList.Count;
            if (total == 0)
                return new BudgetHealthSummaryDto();

            var withinBudget = usageList.Count(u => !u.IsOverLimit);
            var avgPercentUsed = usageList.Average(u => u.PercentageUsed);

            return new BudgetHealthSummaryDto
            {
                TotalCategories = total,
                WithinBudgetCount = withinBudget,
                AverageUsagePercent = avgPercentUsed
            };
        }

        private async Task<decimal> SumConverted(IEnumerable<Transaction> transactions, string toCurrency)
        {
            decimal total = 0;
            foreach (var t in transactions)
            {
                var currency = await _repositoryWrapper.CurrencyRepository.GetCurrencyByCode(toCurrency);
                total += CurrencyHelper.ConvertAmount(t.Amount, t.Currency, currency);
            }
            return total;
        }
    }
}

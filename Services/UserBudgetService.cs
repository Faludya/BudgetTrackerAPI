using Models;
using Repositories.Interfaces;
using Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTOs;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Services
{
    public class UserBudgetService : IUserBudgetService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UserBudgetService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<UserBudget> CreateBudgetFromTemplateAsync(string userId, int month, int year, int templateId, decimal income)
        {
            var template = await _repositoryWrapper.BudgetTemplateRepository.GetTemplateWithItemsAsync(templateId);
            if (template == null) throw new Exception("Template not found");

            var monthDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);

            // Optional: Check if budget already exists
            var query = await _repositoryWrapper.UserBudgetRepository.FindByCondition(b => b.UserId == userId && b.Month == monthDate);
            var existing = query.FirstOrDefault();

            if (existing != null)
            {
                _repositoryWrapper.UserBudgetRepository.Delete(existing);
                await _repositoryWrapper.Save();
            }

            var budget = new UserBudget
            {
                UserId = userId,
                Month = monthDate,
                BudgetItems = template.Items.Select(i => new UserBudgetItem
                {
                    CategoryType = i.CategoryType,
                    Limit = income * i.Percentage / 100,
                    IsAIRecommended = true
                }).ToList()
            };

            _repositoryWrapper.UserBudgetRepository.Create(budget);
            await _repositoryWrapper.Save();

            return budget;
        }

        public async Task<UserBudgetItem?> AddOrUpdateCategoryLimitAsync(CategoryLimitDto categoryDto)
        {
            var monthDate = new DateTime(categoryDto.Year, categoryDto.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var budgets = await _repositoryWrapper.UserBudgetRepository
                .FindByCondition(b => b.UserId == categoryDto.UserId && b.Month == monthDate);

            var budget = await budgets.Include(b => b.BudgetItems).FirstOrDefaultAsync();

            if (budget == null)
            {
                budget = new UserBudget
                {
                    UserId = categoryDto.UserId,
                    Month = monthDate,
                    BudgetItems = new List<UserBudgetItem>()
                };

                _repositoryWrapper.UserBudgetRepository.Create(budget);
                await _repositoryWrapper.Save();

                // Reload
                var bQuery = await _repositoryWrapper.UserBudgetRepository
                    .FindByCondition(b => b.UserId == categoryDto.UserId && b.Month == monthDate);
                budget = await bQuery
                    .Include(b => b.BudgetItems)
                    .FirstOrDefaultAsync();
            }

            // Only update category-specific entries (ignore group-only entries with CategoryId == null)
            var existingItem = budget.BudgetItems.FirstOrDefault(
                bi => bi.CategoryId.HasValue && bi.CategoryId == categoryDto.CategoryId
            );

            if (existingItem != null)
            {
                existingItem.Limit = categoryDto.Limit;
                existingItem.CategoryType = categoryDto.ParentCategoryType;
                existingItem.IsAIRecommended = false;

                await _repositoryWrapper.UserBudgetItemRepository.Update(existingItem);
            }
            else
            {
                var newItem = new UserBudgetItem
                {
                    UserBudgetId = budget.Id,
                    CategoryId = categoryDto.CategoryId,
                    CategoryType = categoryDto.ParentCategoryType,
                    Limit = categoryDto.Limit,
                    IsAIRecommended = false,
                };

                await _repositoryWrapper.UserBudgetItemRepository.Create(newItem);
            }

            await _repositoryWrapper.Save();

            var budgetQuery = await _repositoryWrapper.UserBudgetRepository
                .FindByCondition(b => b.Id == budget.Id);
            budget = await budgetQuery
                .Include(b => b.BudgetItems)
                    .ThenInclude(i => i.Category)
                .FirstOrDefaultAsync();

            return budget?.BudgetItems.FirstOrDefault(b => b.CategoryId == categoryDto.CategoryId);
        }


        public async Task AddBudgetAsync(UserBudget budget)
        {
            await _repositoryWrapper.UserBudgetRepository.Create(budget);
            await _repositoryWrapper.Save();
        }

        public async Task UpdateBudgetAsync(UserBudget budget)
        {
            _repositoryWrapper.UserBudgetRepository.Update(budget);
            await _repositoryWrapper.Save();
        }

        public async Task DeleteBudgetAsync(int id)
        {
            var budget = await _repositoryWrapper.UserBudgetRepository.GetUserBudgetById(id);
            if (budget != null)
            {
                _repositoryWrapper.UserBudgetRepository.Delete(budget);
                await _repositoryWrapper.Save();
            }
        }

        public async Task<UserBudget?> GetBudgetForMonthAsync(string userId, int month, int year)
        {
            var budgets = await _repositoryWrapper.UserBudgetRepository
                .FindByCondition(b => b.UserId == userId && b.Month.Year == year && b.Month.Month == month);

            var budget = await budgets
                .Include(b => b.BudgetItems)
                    .ThenInclude(bi => bi.Category)
                .FirstOrDefaultAsync();

            if (budget == null)
                return null;

            // Optional: project a DTO
            foreach (var item in budget.BudgetItems)
            {
                if (string.IsNullOrEmpty(item.CategoryType) && item.CategoryId.HasValue && item.Category != null)
                {
                    item.CategoryType = item.Category.CategoryType ?? "Uncategorized";
                }
            }

            return budget;
        }


        public async Task<IEnumerable<UserBudget>> GetUserBudgetsAsync(string userId)
        {
            return await _repositoryWrapper.UserBudgetRepository.FindByCondition(b => b.UserId == userId);
        }
    }

}

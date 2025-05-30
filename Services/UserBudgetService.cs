using Models;
using Repositories.Interfaces;
using Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class UserBudgetService : IUserBudgetService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UserBudgetService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
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
            var budgets = await _repositoryWrapper.UserBudgetRepository.FindByCondition(b => b.UserId == userId && b.Month.Year == year && b.Month.Month == month);
            return await budgets.Include(b => b.BudgetItems).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserBudget>> GetUserBudgetsAsync(string userId)
        {
            return await _repositoryWrapper.UserBudgetRepository.FindByCondition(b => b.UserId == userId);
        }
    }

}

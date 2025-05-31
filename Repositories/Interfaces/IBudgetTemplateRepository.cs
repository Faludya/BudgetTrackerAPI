using Models;

namespace Repositories.Interfaces
{
    public interface IBudgetTemplateRepository : IRepositoryBase<BudgetTemplate>
    {
        Task<BudgetTemplate> GetBudgetTemplateById(int id);
        Task<BudgetTemplate?> GetTemplateWithItemsAsync(int id);
        Task<List<BudgetTemplate>> GetAllBudgetTemplatesAsync();
    }
}

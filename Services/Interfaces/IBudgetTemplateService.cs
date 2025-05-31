using Models;

namespace Services.Interfaces
{
    public interface IBudgetTemplateService
    {
        Task<IEnumerable<BudgetTemplate>> GetAllTemplatesAsync();
        Task<BudgetTemplate?> GetTemplateByIdAsync(int id);
        Task<BudgetTemplate?> GetTemplateWithItemsAsync(int templateId);

        Task AddTemplateAsync(BudgetTemplate template);
        Task UpdateTemplateAsync(BudgetTemplate template);
        Task DeleteTemplateAsync(int id);
    }

}

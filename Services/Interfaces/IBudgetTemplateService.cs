using Models;

namespace Services.Interfaces
{
    public interface IBudgetTemplateService
    {
        Task<IEnumerable<BudgetTemplate>> GetAllTemplatesAsync();
        Task<BudgetTemplate?> GetTemplateByIdAsync(int id);
        Task AddTemplateAsync(BudgetTemplate template);
        Task UpdateTemplateAsync(BudgetTemplate template);
        Task DeleteTemplateAsync(int id);
    }

}

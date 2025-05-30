using Models;

namespace Services.Interfaces
{
    public interface IBudgetTemplateItemService
    {
        Task<IEnumerable<BudgetTemplateItem>> GetItemsByTemplateIdAsync(int templateId);
        Task AddItemAsync(BudgetTemplateItem item);
        Task UpdateItemAsync(BudgetTemplateItem item);
        Task DeleteItemAsync(int id);
    }

}

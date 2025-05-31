using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class BudgetTemplateItemRepository : RepositoryBase<BudgetTemplateItem>, IBudgetTemplateItemRepository
    {
        public BudgetTemplateItemRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<BudgetTemplateItem> GetBudgetTemplateItemById(int id)
        {
            return await _appDbContext.BudgetTemplateItems.FindAsync(id);
        }

        public async Task<BudgetTemplate?> GetTemplateWithItemsAsync(int templateId)
        {
            return await _appDbContext.BudgetTemplates.Include(t => t.Items).FirstOrDefaultAsync(t => t.Id == templateId);
        }

    }
}

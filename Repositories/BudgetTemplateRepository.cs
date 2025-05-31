using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class BudgetTemplateRepository : RepositoryBase<BudgetTemplate>, IBudgetTemplateRepository
    {
        public BudgetTemplateRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<List<BudgetTemplate>> GetAllBudgetTemplatesAsync()
        {
            return await _appDbContext.BudgetTemplates.Include(t => t.Items).ToListAsync();
        }

        public async Task<BudgetTemplate> GetBudgetTemplateById(int id)
        {
            return await _appDbContext.BudgetTemplates.FindAsync(id);
        }

        public async Task<BudgetTemplate?> GetTemplateWithItemsAsync(int id)
        {
            return await _appDbContext.BudgetTemplates
                .Include(t => t.Items)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

    }
}

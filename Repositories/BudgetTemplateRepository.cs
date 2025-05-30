using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class BudgetTemplateRepository : RepositoryBase<BudgetTemplate>, IBudgetTemplateRepository
    {
        public BudgetTemplateRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public Task<List<BudgetTemplate>> GetAllBudgetTemplatesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<BudgetTemplate> GetBudgetTemplateById(int id)
        {
            return await _appDbContext.BudgetTemplates.FindAsync(id);
        }
    }
}

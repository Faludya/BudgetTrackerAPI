using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class BudgetTemplateService : IBudgetTemplateService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public BudgetTemplateService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<IEnumerable<BudgetTemplate>> GetAllTemplatesAsync()
        {
            return await _repositoryWrapper.BudgetTemplateRepository.GetAllBudgetTemplatesAsync();
        }

        public async Task<BudgetTemplate> GetTemplateByIdAsync(int id)
        {
            return await _repositoryWrapper.BudgetTemplateRepository.GetBudgetTemplateById(id);
        }

        public async Task AddTemplateAsync(BudgetTemplate template)
        {
            await _repositoryWrapper.BudgetTemplateRepository.Create(template);
            await _repositoryWrapper.Save();
        }

        public async Task UpdateTemplateAsync(BudgetTemplate template)
        {
            _repositoryWrapper.BudgetTemplateRepository.Update(template);
            await _repositoryWrapper.Save();
        }

        public async Task DeleteTemplateAsync(int id)
        {
            var template = await _repositoryWrapper.BudgetTemplateRepository.GetBudgetTemplateById(id);
            if (template != null)
            {
                _repositoryWrapper.BudgetTemplateRepository.Delete(template);
                await _repositoryWrapper.Save();
            }
        }

        public async Task<BudgetTemplate?> GetTemplateWithItemsAsync(int templateId)
        {
            return await _repositoryWrapper.BudgetTemplateRepository.GetTemplateWithItemsAsync(templateId);
        }
    }

}

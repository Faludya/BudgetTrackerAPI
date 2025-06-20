﻿using Models;

namespace Repositories.Interfaces
{
    public interface IBudgetTemplateItemRepository : IRepositoryBase<BudgetTemplateItem>
    {
        Task<BudgetTemplateItem> GetBudgetTemplateItemById(int id);
        Task<BudgetTemplate?> GetTemplateWithItemsAsync(int templateId);

    }
}

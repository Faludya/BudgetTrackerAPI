using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class DashboardLayoutRepository : RepositoryBase<DashboardLayout>, IDashboardLayoutRepository
    {
        public DashboardLayoutRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<DashboardLayout?> GetByUserIdAsync(string userId)
        {
            return await _appDbContext.DashboardLayouts.Where(u => u.UserId == userId).FirstAsync();
        }
        public async Task SaveOrUpdateAsync(DashboardLayout layout)
        {
            var existing = await _appDbContext.DashboardLayouts.FirstOrDefaultAsync(dl => dl.UserId == layout.UserId);

            if (existing != null)
            {
                existing.WidgetOrder = layout.WidgetOrder;
                _appDbContext.DashboardLayouts.Update(existing);
            }
            else
            {
                await _appDbContext.DashboardLayouts.AddAsync(layout);
            }
        }
    }
}

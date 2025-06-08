using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IDashboardLayoutRepository : IRepositoryBase<DashboardLayout>
    {
        Task<DashboardLayout?> GetByUserIdAsync(string userId);
        Task SaveOrUpdateAsync(DashboardLayout layout);
    }
}

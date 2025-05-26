using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IImportSessionRepository : IRepositoryBase<ImportSession>
    {
        Task<ImportSession?> GetImportSessionById(Guid id);
        Task<List<ImportSession>> GetAllSessionsForUserAsync(string userId);
    }
}

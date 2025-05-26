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
    public class ImportSessionRepository : RepositoryBase<ImportSession>, IImportSessionRepository
    {
        public ImportSessionRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<ImportSession?> GetImportSessionById(Guid id)
        {
            return await _appDbContext.ImportSessions.Include(s => s.Transactions).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<ImportSession>> GetAllSessionsForUserAsync(string userId)
        {
            return await _appDbContext.ImportSessions.Include(s => s.Transactions).Where(s => s.UserId == userId).ToListAsync();
        }

    }
}

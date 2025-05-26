using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class ImportedTransactionRepository : RepositoryBase<ImportedTransaction>, IImportedTransactionRepository
    {
        public ImportedTransactionRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<ImportedTransaction?> GetByIdAndSessionAsync(int transactionId, Guid sessionId)
        {
            return await _appDbContext.ImportedTransactions.FirstOrDefaultAsync(tx => tx.Id == transactionId && tx.ImportSessionId == sessionId);
        }

        public async Task<ImportedTransaction?> GetImportedTransactionById(int id)
        {
            return await _appDbContext.ImportedTransactions.FirstOrDefaultAsync(tx => tx.Id == id);
        }
    }
}

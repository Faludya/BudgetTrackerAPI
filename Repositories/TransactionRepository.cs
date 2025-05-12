using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
    {
        public TransactionRepository(Models.AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<Transaction> GetTransactionById(int transactionId)
        {
            return await _appDbContext.Transactions.FindAsync(transactionId);
        }

        public async Task<List<Transaction>> GetTransactions(string userId)
        {
            return await _appDbContext.Transactions.Include(t => t.Category).Include(t => t.Currency).Where(t => t.UserId == userId).ToListAsync();
        }
    }
}

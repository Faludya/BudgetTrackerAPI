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
    }
}

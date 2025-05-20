using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTOs;
using Repositories.Interfaces;
using static NuGet.Packaging.PackagingConstants;

namespace Repositories
{
    public class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
    {
        public TransactionRepository(Models.AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<IEnumerable<Transaction>> GetFilteredTransactions(string userId, TransactionFilterDto filters)
        {
            var query = _appDbContext.Transactions
                .Include(t => t.Category)
                .Include(t => t.Currency)
                .Where(t => t.UserId == userId);

            if (filters.FromDate.HasValue)
                query = query.Where(t => t.Date >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(t => t.Date <= filters.ToDate.Value);

            if (!string.IsNullOrEmpty(filters.Type))
                query = query.Where(t => t.Type == filters.Type);

            if (filters.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filters.CategoryId.Value);

            return await query.ToListAsync();
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

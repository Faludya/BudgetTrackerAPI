using Models;
using Models.DTOs;

namespace Repositories.Interfaces
{
    public interface ITransactionRepository : IRepositoryBase<Transaction>
    {
        Task<List<Transaction>> GetTransactions(string userId);
        Task<IEnumerable<Transaction>> GetFilteredTransactions(string userId, TransactionFilterDto transactionFilterDto);
        Task<Transaction> GetTransactionById(int transactionId);
    }
}

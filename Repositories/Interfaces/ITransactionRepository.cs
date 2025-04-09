using Models;

namespace Repositories.Interfaces
{
    public interface ITransactionRepository : IRepositoryBase<Transaction>
    {
        Task<List<Transaction>> GetTransactions(string userId);
        Task<Transaction> GetTransactionById(int transactionId);
    }
}

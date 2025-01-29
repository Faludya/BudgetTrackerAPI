using Models;

namespace Repositories.Interfaces
{
    public interface ITransactionRepository : IRepositoryBase<Transaction>
    {
        Task<Transaction> GetTransactionById(int transactionId);
    }
}

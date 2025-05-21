using Models;
using Models.DTOs;

namespace Services.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync(string userId);
        Task<IEnumerable<Transaction>> GetAllFilteredTransactions(string userId, TransactionFilterDto transactionFilterDto);
        Task<Transaction> GetTransactionByIdAsync(int id);
        Task AddTransactionAsync(Transaction transaction);
        Task UpdateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(int id);
        Task<ExportFileResult?> ExportAsync(string userId, TransactionFilterDto filters, string format);

    }
}

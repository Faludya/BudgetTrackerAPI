using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public TransactionService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _repositoryWrapper.TransactionRepository.FindAll();
        }

        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            return await _repositoryWrapper.TransactionRepository.GetTransactionById(id)
                   ?? throw new KeyNotFoundException($"Transaction with ID {id} not found.");
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null.");
            }

            // Validate Foreign Key Dependencies
            var categoryExists = await _repositoryWrapper.CategoryRepository.GetCategoryById(transaction.CategoryId);
            var currencyExists = await _repositoryWrapper.CurrencyRepository.GetCurrencyById(transaction.CurrencyId);
            var userExists = await _repositoryWrapper.ApplicationUserRepository.GetUserByIdAsync(transaction.UserId);

            if (categoryExists == null)
                throw new KeyNotFoundException($"Category with ID {transaction.CategoryId} not found.");

            if (currencyExists == null)
                throw new KeyNotFoundException($"Currency with ID {transaction.CurrencyId} not found.");

            if (userExists == null)
                throw new KeyNotFoundException($"User with ID {transaction.UserId} not found.");

            transaction.Date = transaction.Date.ToUniversalTime();
            transaction.CreatedAt = DateTime.UtcNow;

            await _repositoryWrapper.TransactionRepository.Create(transaction);
            await _repositoryWrapper.Save();
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null.");
            }

            var existingTransaction = await _repositoryWrapper.TransactionRepository.GetTransactionById(transaction.Id);
            if (existingTransaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {transaction.Id} not found.");
            }

            // Validate Foreign Key Dependencies
            var categoryExists = await _repositoryWrapper.CategoryRepository.GetCategoryById(transaction.CategoryId);
            var currencyExists = await _repositoryWrapper.CurrencyRepository.GetCurrencyById(transaction.CurrencyId);
            var userExists = await _repositoryWrapper.ApplicationUserRepository.GetUserByIdAsync(transaction.UserId);

            if (categoryExists == null)
                throw new KeyNotFoundException($"Category with ID {transaction.CategoryId} not found.");

            if (currencyExists == null)
                throw new KeyNotFoundException($"Currency with ID {transaction.CurrencyId} not found.");

            if (userExists == null)
                throw new KeyNotFoundException($"User with ID {transaction.UserId} not found.");

            // Update only allowed fields
            existingTransaction.Type = transaction.Type;
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.Description = transaction.Description;
            existingTransaction.Date = transaction.Date;
            existingTransaction.CategoryId = transaction.CategoryId;
            existingTransaction.CurrencyId = transaction.CurrencyId;
            existingTransaction.UserId = transaction.UserId;

            _repositoryWrapper.TransactionRepository.Update(existingTransaction);
            await _repositoryWrapper.Save();
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await _repositoryWrapper.TransactionRepository.GetTransactionById(id);
            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");
            }

            await _repositoryWrapper.TransactionRepository.Delete(transaction);
            await _repositoryWrapper.Save();
        }
    }

}

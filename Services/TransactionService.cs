using Models;
using Models.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;
using static NuGet.Packaging.PackagingConstants;

namespace Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public TransactionService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(string userId)
        {
            var transactions = (await _repositoryWrapper.TransactionRepository.GetTransactions(userId)).ToList();
            var preferences = await _repositoryWrapper.UserPreferencesRepository.GetUserPreferences(userId);
            var preferredCurrency = await _repositoryWrapper.CurrencyRepository.GetCurrencyByCode(preferences.PreferredCurrency);

            foreach (var t in transactions)
            {
                if (t.Currency?.Code == preferredCurrency.Code)
                {
                    t.ConvertedAmount = t.Amount;
                    continue;
                }

                var originalCurrency = t.Currency;
                if (originalCurrency == null || originalCurrency.ExchangeRate == 0)
                {
                    t.ConvertedAmount = t.Amount; // fallback
                    continue;
                }

                var baseAmount = t.Amount / originalCurrency.ExchangeRate;
                t.ConvertedAmount = Math.Round(baseAmount * preferredCurrency.ExchangeRate, 2);
            }

            return transactions;
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

        public async Task<IEnumerable<Transaction>> GetAllFilteredTransactions(string userId, TransactionFilterDto dto)
        {
            var transactions = (await _repositoryWrapper.TransactionRepository.GetFilteredTransactions(userId, dto)).ToList();
            var preferences = await _repositoryWrapper.UserPreferencesRepository.GetUserPreferences(userId);
            if (preferences == null)
                throw new InvalidOperationException("User preferences not found.");

            var preferredCurrency = await _repositoryWrapper.CurrencyRepository.GetCurrencyByCode(preferences.PreferredCurrency);

            foreach (var t in transactions)
            {
                if (t.Currency?.Code == preferredCurrency.Code)
                {
                    t.ConvertedAmount = t.Amount;
                }
                else if (t.Currency == null || t.Currency.ExchangeRate == 0)
                {
                    t.ConvertedAmount = t.Amount;
                }
                else
                {
                    var baseAmount = t.Amount / t.Currency.ExchangeRate;
                    t.ConvertedAmount = Math.Round(baseAmount * preferredCurrency.ExchangeRate, 2);
                }
            }

            if (dto.AmountMin.HasValue)
                transactions = transactions.Where(t => t.ConvertedAmount >= dto.AmountMin.Value).ToList();

            if (dto.AmountMax.HasValue)
                transactions = transactions.Where(t => t.ConvertedAmount <= dto.AmountMax.Value).ToList();

            return transactions;
        }

        public async Task<ExportFileResult?> ExportAsync(string userId, TransactionFilterDto filters, string format)
        {
            var transactions = await GetAllFilteredTransactions(userId, filters);

            return format switch
            {
                "excel" => ExportService.ExportToExcel(transactions),
                "pdf" => ExportService.ExportToPdf(transactions),
                _ => null
            };
        }

    }

}

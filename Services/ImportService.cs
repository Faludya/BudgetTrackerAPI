using Microsoft.AspNetCore.Http;
using Models;
using Models.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class ImportService : IImportService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IImportParserService _parserService;

        public ImportService(IRepositoryWrapper repositoryWrapper, IImportParserService parserService)
        {
            _repositoryWrapper = repositoryWrapper;
            _parserService = parserService;
        }

        public async Task<ImportSession?> GetImportSessionAsync(Guid id)
        {
            return await _repositoryWrapper.ImportSessionRepository.GetImportSessionById(id);
        }

        public async Task<string> CompleteImportAsync(Guid sessionId, string userId)
        {
            var session = await _repositoryWrapper.ImportSessionRepository.GetImportSessionById(sessionId);
            if (session == null || session.IsCompleted)
                throw new InvalidOperationException("Invalid or already completed session.");

            foreach (var tx in session.Transactions)
            {
                var category = await _repositoryWrapper.CategoryRepository.GetCategoryByUserIdAndName(userId, tx.Category);
                var currency = await _repositoryWrapper.CurrencyRepository.GetCurrencyByCode(tx.Currency);

                if (category == null || currency == null)
                    continue; 

                await _repositoryWrapper.TransactionRepository.Create(new Transaction
                {
                    UserId = userId,
                    Date = tx.Date,
                    Description = tx.Description,
                    Amount = tx.Amount,
                    CategoryId = category.Id,
                    CurrencyId = currency.Id,
                    CreatedAt = DateTime.UtcNow,
                    Type = tx.Amount < 0 ? "Debit" : "Credit"
                });
            }

            session.IsCompleted = true;
            await _repositoryWrapper.ImportSessionRepository.Update(session);
            await _repositoryWrapper.Save();

            return "Import completed.";
        }

        public async Task<ImportSession> CreateImportSessionAsync(IFormFile file, string template, string userId)
        {
            var stream = file.OpenReadStream();
            var parsedTransactions = await _parserService.ParseAsync(stream, template);

            var session = new ImportSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Template = template,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false,
            };

            await _repositoryWrapper.ImportSessionRepository.Create(session);
            await _repositoryWrapper.Save();

            foreach (var parsedTx in parsedTransactions)
            {
                try
                {
                    var importedTx = new ImportedTransaction
                    {
                        ImportSessionId = session.Id,
                        Date = parsedTx.Date.ToUniversalTime(),
                        Description = parsedTx.Description,
                        Amount = parsedTx.Amount,
                        Currency = parsedTx.Currency,
                        Category = parsedTx.Category
                    };

                    await _repositoryWrapper.ImportedTransactionRepository.Create(importedTx);
                    await _repositoryWrapper.Save();

                    // Category suggestion...
                    var keywordCategory = await FindCategorySuggestionFromKeyword(parsedTx.Description, userId);
                    if (keywordCategory != null)
                    {
                        var suggestion = new CategorySuggestion
                        {
                            ImportedTransactionId = importedTx.Id,
                            CategoryId = keywordCategory.Value.CategoryId,
                            Confidence = 0.85m,
                            IsFromMLModel = false,
                            SourceKeyword = keywordCategory.Value.Keyword
                        };

                        await _repositoryWrapper.CategorySuggestionRepository.Create(suggestion);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error while saving transaction: {ex.Message}");
                }
            }

            await _repositoryWrapper.Save();
            return session;
        }


        public async Task<bool> UpdateImportedTransactionAsync(Guid sessionId, int transactionId, UpdateImportedTransactionDto dto)
        {
            var tx = await _repositoryWrapper.ImportedTransactionRepository.GetByIdAndSessionAsync(transactionId, sessionId);
            if (tx == null) return false;

            tx.Date = dto.Date;
            tx.Description = dto.Description;
            tx.Amount = dto.Amount;
            tx.Currency = dto.Currency;
            tx.Category = dto.Category;

            await _repositoryWrapper.ImportedTransactionRepository.Update(tx);
            await _repositoryWrapper.Save();

            return true;
        }

        public async Task<List<ImportSession>> GetAllSessionsForUserAsync(string userId)
        {
            return await _repositoryWrapper.ImportSessionRepository.GetAllSessionsForUserAsync(userId);
        }

        private async Task<(int CategoryId, string Keyword)?> FindCategorySuggestionFromKeyword(string description, string userId)
        {
            if (string.IsNullOrWhiteSpace(description)) return null;

            var words = description.ToLower().Split(' ', '.', ',', '/', '\\', '-', '_');

            foreach (var word in words)
            {
                var categoryId = await _repositoryWrapper.CategoryRepository.GetCategoryByUserIdAndName(userId, word);

                if (categoryId != null)
                {
                    return (categoryId.Id, word);
                }
            }

            return null;
        }

    }
}

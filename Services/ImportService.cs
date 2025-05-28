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
        private readonly ICategorySuggestionRepository _categorySuggestionRepository;

        public ImportService(IRepositoryWrapper repositoryWrapper, IImportParserService parserService, ICategorySuggestionRepository categorySuggestionRepository)
        {
            _repositoryWrapper = repositoryWrapper;
            _parserService = parserService;
            _categorySuggestionRepository = categorySuggestionRepository;
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

                // 👇 Save Category Suggestion if not already existing
                var existingSuggestion = await _repositoryWrapper.CategorySuggestionRepository.GetCategorySuggestionById(tx.Id);

                if (existingSuggestion == null)
                {
                    var suggestion = new CategorySuggestion
                    {
                        ImportedTransactionId = tx.Id,
                        CategoryId = category.Id,
                        Confidence = 1.0m,
                        IsFromMLModel = false,
                        SourceKeyword = null
                    };
                    await _repositoryWrapper.CategorySuggestionRepository.Create(suggestion);
                }
                else if (existingSuggestion.CategoryId != category.Id)
                {
                    // update the suggestion if user changed it
                    existingSuggestion.CategoryId = category.Id;
                    existingSuggestion.Confidence = 1.0m;
                    existingSuggestion.IsFromMLModel = false;
                    existingSuggestion.SourceKeyword = null;

                    await _repositoryWrapper.CategorySuggestionRepository.Update(existingSuggestion);
                }
            }

            await _repositoryWrapper.ImportSessionRepository.Delete(session);
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
                string? suggestedCategory = null;
                var keywordCategory = await FindCategorySuggestionFromKeyword(parsedTx.Description, userId);

                if (keywordCategory != null)
                {
                    suggestedCategory = (await _repositoryWrapper.CategoryRepository
                        .GetCategoryById(keywordCategory.Value.CategoryId))?.Name;
                }

                var finalCategory = suggestedCategory ?? parsedTx.Category;

                var importedTx = new ImportedTransaction
                {
                    ImportSessionId = session.Id,
                    Date = parsedTx.Date,
                    Description = parsedTx.Description,
                    Amount = parsedTx.Amount,
                    Currency = parsedTx.Currency,
                    Category = finalCategory
                };

                await _repositoryWrapper.ImportedTransactionRepository.Create(importedTx);
                await _repositoryWrapper.Save();

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
                else if (!string.IsNullOrEmpty(finalCategory))
                {
                    var matchedCategory = await _repositoryWrapper.CategoryRepository
                        .GetCategoryByUserIdAndName(userId, finalCategory);

                    if (matchedCategory != null)
                    {
                        // Extract a keyword from the description for future matching
                        var keyword = parsedTx.Description?
                            .ToLower()
                            .Split(new[] { ' ', '|', '-', ',', '.', ':' }, StringSplitOptions.RemoveEmptyEntries)
                            .FirstOrDefault(w => w.Length > 3);

                        var fallbackSuggestion = new CategorySuggestion
                        {
                            ImportedTransactionId = importedTx.Id,
                            CategoryId = matchedCategory.Id,
                            Confidence = 1.0m,
                            IsFromMLModel = false,
                            SourceKeyword = keyword
                        };

                        await _repositoryWrapper.CategorySuggestionRepository.Create(fallbackSuggestion);
                    }
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

        public async Task<ImportSession> GetImportSessionsByUserIdAsync(string userId)
        {
            return await _repositoryWrapper.ImportSessionRepository.GetImportSessionsByUserId(userId);
        }

        public async Task UpdateImportSessionAsync(Guid sessionId, List<UpdateImportedTransactionDto> transactions)
        {
            foreach (var dto in transactions)
            {
                var tx = await _repositoryWrapper.ImportedTransactionRepository.GetImportedTransactionById(dto.Id);
                if (tx != null)
                {
                    tx.Date = dto.Date;
                    tx.Description = dto.Description;
                    tx.Amount = dto.Amount;
                    tx.Currency = dto.Currency;
                    tx.Category = dto.Category;

                    await _repositoryWrapper.ImportedTransactionRepository.Update(tx);
                }
            }

            await _repositoryWrapper.Save();
        }

        public async Task<(int CategoryId, string Keyword)?> FindCategorySuggestionFromKeyword(string description, string userId)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            var words = description.ToLower().Split(new[] { ' ', '|', '-', ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var suggestion = await _repositoryWrapper.CategorySuggestionRepository.FindSuggestionByKeywordAsync(userId, word);

                if (suggestion != null)
                    return (suggestion.CategoryId, word);
            }

            return null;
        }


    }
}

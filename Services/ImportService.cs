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
        private readonly ICategoryKeywordMappingService _mappingService;
        private readonly ICategoryService _categoryService;

        public ImportService(IRepositoryWrapper repositoryWrapper, IImportParserService parserService, 
            ICategorySuggestionRepository categorySuggestionRepository, ICategoryKeywordMappingService mappingService, ICategoryService categoryService)
        {
            _repositoryWrapper = repositoryWrapper;
            _parserService = parserService;
            _categorySuggestionRepository = categorySuggestionRepository;
            _mappingService = mappingService;
            _categoryService = categoryService;
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
                    Amount = Math.Abs(tx.Amount),
                    CategoryId = category.Id,
                    CurrencyId = currency.Id,
                    CreatedAt = DateTime.UtcNow,
                    Type = tx.Amount < 0 ? "Debit" : "Credit"
                });

                // Category Suggestion logic
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
                    existingSuggestion.CategoryId = category.Id;
                    existingSuggestion.Confidence = 1.0m;
                    existingSuggestion.IsFromMLModel = false;
                    existingSuggestion.SourceKeyword = null;

                    await _repositoryWrapper.CategorySuggestionRepository.Update(existingSuggestion);
                }

                //Save keyword mapping if user opted to remember it
                if (tx.RememberMapping && !string.IsNullOrWhiteSpace(tx.Description))
                {
                    var keyword = _mappingService.ExtractFirstMeaningfulWord(tx.Description);
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        var exists = await _repositoryWrapper.CategoryKeywordMappingRepository
                            .FindByCondition(m =>
                                m.UserId == userId &&
                                m.Keyword.ToLower() == keyword.ToLower())
                            .ContinueWith(t => t.Result.Any());

                        if (!exists)
                        {
                            var newMapping = new CategoryKeywordMapping
                            {
                                UserId = userId,
                                Keyword = keyword,
                                CategoryId = category.Id
                            };
                            await _repositoryWrapper.CategoryKeywordMappingRepository.Create(newMapping);
                        }
                    }
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
                var matchedCategoryId = await _mappingService.FindCategoryIdByKeywordAsync(userId, parsedTx.Description);
                string? finalCategory = null;

                if (matchedCategoryId.HasValue)
                {
                    var matchedCategory = await _repositoryWrapper.CategoryRepository.GetCategoryById(matchedCategoryId.Value);
                    finalCategory = matchedCategory?.Name;
                }

                var importedTx = new ImportedTransaction
                {
                    ImportSessionId = session.Id,
                    Date = DateTime.SpecifyKind(parsedTx.Date, DateTimeKind.Utc),
                    Description = parsedTx.Description,
                    Amount = parsedTx.Amount,
                    Currency = parsedTx.Currency,
                    Category = finalCategory,
                    IsFromMLModel = false
                };

                await _repositoryWrapper.ImportedTransactionRepository.Create(importedTx);
                await _repositoryWrapper.Save();

                if (matchedCategoryId.HasValue)
                {
                    var keyword = _mappingService.ExtractFirstMeaningfulWord(parsedTx.Description);
                    var suggestion = new CategorySuggestion
                    {
                        ImportedTransactionId = importedTx.Id,
                        CategoryId = matchedCategoryId.Value,
                        Confidence = 1.0m,
                        IsFromMLModel = false,
                        SourceKeyword = keyword
                    };

                    await _repositoryWrapper.CategorySuggestionRepository.Create(suggestion);
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
            tx.RememberMapping = dto.RememberMapping;

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
                    tx.RememberMapping = dto.RememberMapping;

                    await _repositoryWrapper.ImportedTransactionRepository.Update(tx);
                }
            }

            await _repositoryWrapper.Save();
        }
    }
}

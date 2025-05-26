using Microsoft.AspNetCore.Identity;
using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly Lazy<IApplicationUserRepository> _applicationUserRepository;
        private readonly Lazy<ICategoryRepository> _categoryRepository;
        private readonly Lazy<ITransactionRepository> _transactionRepository;
        private readonly Lazy<ICurrencyRepository> _currencyRepository;
        private readonly Lazy<IUserPreferencesRepository> _userPreferencesRepository;
        private readonly Lazy<IImportSessionRepository> _importSessionRepository;
        private readonly Lazy<IImportedTransactionRepository> _importedTransactionRepository;
        private readonly Lazy<ICategorySuggestionRepository> _categorySuggestionRepository;

        public IApplicationUserRepository ApplicationUserRepository => _applicationUserRepository.Value;
        public ICategoryRepository CategoryRepository => _categoryRepository.Value;
        public ITransactionRepository TransactionRepository => _transactionRepository.Value;
        public ICurrencyRepository CurrencyRepository => _currencyRepository.Value;
        public IUserPreferencesRepository UserPreferencesRepository => _userPreferencesRepository.Value;
        public IImportSessionRepository ImportSessionRepository=> _importSessionRepository.Value;
        public IImportedTransactionRepository ImportedTransactionRepository => _importedTransactionRepository.Value;
        public ICategorySuggestionRepository CategorySuggestionRepository => _categorySuggestionRepository.Value;

        public RepositoryWrapper(AppDbContext appDbContext, UserManager<ApplicationUser> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;

            _userPreferencesRepository = new(() => new UserPreferencesRepository(_appDbContext));
            _categoryRepository = new(() => new CategoryRepository(_appDbContext));
            _transactionRepository = new(() => new TransactionRepository(_appDbContext));
            _currencyRepository = new(() => new CurrencyRepository(_appDbContext));
            _applicationUserRepository = new(() => new ApplicationUserRepository(_userManager, UserPreferencesRepository));
            _importedTransactionRepository = new(() => new ImportedTransactionRepository(_appDbContext));
            _importSessionRepository = new(() => new ImportSessionRepository(_appDbContext));
            _categorySuggestionRepository = new(() => new CategorySuggestionRepository(_appDbContext));
        }

        public async Task Save()
        {
            await _appDbContext.SaveChangesAsync();
        }
    }
}

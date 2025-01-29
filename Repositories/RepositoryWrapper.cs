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

        public IApplicationUserRepository ApplicationUserRepository => _applicationUserRepository.Value;
        public ICategoryRepository CategoryRepository => _categoryRepository.Value;
        public ITransactionRepository TransactionRepository => _transactionRepository.Value;
        public ICurrencyRepository CurrencyRepository => _currencyRepository.Value;

        public RepositoryWrapper(AppDbContext appDbContext, UserManager<ApplicationUser> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;

            _applicationUserRepository = new(() => new ApplicationUserRepository(_userManager));
            _categoryRepository = new(() => new CategoryRepository(_appDbContext));
            _transactionRepository = new(() => new TransactionRepository(_appDbContext));
            _currencyRepository = new(() => new CurrencyRepository(_appDbContext));
        }

        public async Task Save()
        {
            await _appDbContext.SaveChangesAsync();
        }
    }
}

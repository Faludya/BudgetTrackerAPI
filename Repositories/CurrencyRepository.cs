using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class CurrencyRepository : RepositoryBase<Currency>, ICurrencyRepository
    {
        public CurrencyRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<Currency> GetCurrencyByCode(string code)
        {
            return await _appDbContext.Currencies.Where(c => c.Code == code).FirstAsync();
        }

        public async Task<Currency> GetCurrencyById(int id)
        {
            return await _appDbContext.Currencies.FindAsync(id);
        }
    }
}

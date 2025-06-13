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

        public async Task<CurrencyExchangeRate?> GetHistoricalRateAsync(int baseCurrencyId, int targetCurrencyId, DateTime date)
        {
            return await _appDbContext.CurrencyExchangeRates
                .FirstOrDefaultAsync(r =>
                    r.BaseCurrencyId == baseCurrencyId &&
                    r.TargetCurrencyId == targetCurrencyId &&
                    r.Date.Date == date.Date);
        }

        public async Task AddExchangeRateAsync(CurrencyExchangeRate rate)
        {
            await _appDbContext.CurrencyExchangeRates.AddAsync(rate);
        }

        public async Task<CurrencyExchangeRate?> GetLatestRateBeforeDateAsync(int baseCurrencyId, int targetCurrencyId, DateTime date)
        {
            return await _appDbContext.CurrencyExchangeRates
                .Where(r =>
                    r.BaseCurrencyId == baseCurrencyId &&
                    r.TargetCurrencyId == targetCurrencyId &&
                    r.Date < date)
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();
        }

        public async Task<List<CurrencyExchangeRate>> GetExchangeRateHistoryAsync(int targetCurrencyId, DateTime fromDate)
        {
            var utcFromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);

            return await _appDbContext.CurrencyExchangeRates
                .Where(r => r.BaseCurrency.Id == 2 &&
                            r.TargetCurrencyId == targetCurrencyId &&
                            r.Date >= utcFromDate)
                .OrderBy(r => r.Date)
                .ToListAsync();
        }
    }
}

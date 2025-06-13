using Models;

namespace Repositories.Interfaces
{
    public interface ICurrencyRepository : IRepositoryBase<Currency>
    {
        Task<Currency> GetCurrencyById(int id);
        Task<Currency> GetCurrencyByCode(string code);
        Task<CurrencyExchangeRate?> GetHistoricalRateAsync(int baseCurrencyId, int targetCurrencyId, DateTime date);
        Task AddExchangeRateAsync(CurrencyExchangeRate rate);
        Task<CurrencyExchangeRate?> GetLatestRateBeforeDateAsync(int baseCurrencyId, int targetCurrencyId, DateTime date);
        Task<List<CurrencyExchangeRate>> GetExchangeRateHistoryAsync(int targetCurrencyId, DateTime fromDate);
    }
}

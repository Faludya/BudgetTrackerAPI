using Models;

namespace Services.Interfaces
{
    public interface ICurrencyService
    {
        Task<IEnumerable<Currency>> GetAllCurrenciesAsync();
        Task<Currency> GetCurrencyByIdAsync(int id);
        Task AddCurrencyAsync(Currency currency);
        Task UpdateCurrencyAsync(Currency currency);
        Task DeleteCurrencyAsync(int id);
    }
}

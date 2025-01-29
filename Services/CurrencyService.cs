using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public CurrencyService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync()
        {
            return await _repositoryWrapper.CurrencyRepository.FindAll();
        }

        public async Task<Currency> GetCurrencyByIdAsync(int id)
        {
            return await _repositoryWrapper.CurrencyRepository.GetCurrencyById(id)
                   ?? throw new KeyNotFoundException($"Currency with ID {id} not found.");
        }

        public async Task AddCurrencyAsync(Currency currency)
        {
            if (currency == null)
            {
                throw new ArgumentNullException(nameof(currency), "Currency cannot be null.");
            }

            // Ensure the currency code is always uppercase (e.g., "usd" → "USD")
            currency.Code = currency.Code.ToUpper();

            await _repositoryWrapper.CurrencyRepository.Create(currency);
            await _repositoryWrapper.Save();
        }

        public async Task UpdateCurrencyAsync(Currency currency)
        {
            if (currency == null)
            {
                throw new ArgumentNullException(nameof(currency), "Currency cannot be null.");
            }

            var existingCurrency = await _repositoryWrapper.CurrencyRepository.GetCurrencyById(currency.Id);
            if (existingCurrency == null)
            {
                throw new KeyNotFoundException($"Currency with ID {currency.Id} not found.");
            }

            existingCurrency.Code = currency.Code.ToUpper();
            existingCurrency.Name = currency.Name;
            existingCurrency.Symbol = currency.Symbol;
            existingCurrency.ExchangeRate = currency.ExchangeRate;

            await _repositoryWrapper.CurrencyRepository.Update(existingCurrency);
            await _repositoryWrapper.Save();
        }

        public async Task DeleteCurrencyAsync(int id)
        {
            var currency = await _repositoryWrapper.CurrencyRepository.GetCurrencyById(id);
            if (currency == null)
            {
                throw new KeyNotFoundException($"Currency with ID {id} not found.");
            }

            await _repositoryWrapper.CurrencyRepository.Delete(currency);
            await _repositoryWrapper.Save();
        }
    }

}

using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ICurrencyApiService _currencyApiService;

        public CurrencyService(IRepositoryWrapper repositoryWrapper, ICurrencyApiService currencyApiService)
        {
            _repositoryWrapper = repositoryWrapper;
            _currencyApiService = currencyApiService;
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
        public async Task<decimal> GetExchangeRateAsync(int targetCurrencyId, DateTime date)
        {
            var targetCurrency = await _repositoryWrapper.CurrencyRepository.GetCurrencyById(targetCurrencyId);
            var baseCurrency = await _repositoryWrapper.CurrencyRepository.GetCurrencyByCode("EUR");
            if (targetCurrency.Code == "EUR") return 1m;

            var existingRate = await _repositoryWrapper.CurrencyRepository.GetHistoricalRateAsync(baseCurrency.Id, targetCurrencyId, date);
            if (existingRate != null)
                return existingRate.Rate;

            // Try to fetch from Frankfurter
            decimal? rate = null;
            try
            {
                rate = await _currencyApiService.GetRateAsync(targetCurrency.Code, date);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Frankfurter API failed: {ex.Message}");
            }

            if (rate.HasValue)
            {
                var newRate = new CurrencyExchangeRate
                {
                    BaseCurrencyId = baseCurrency.Id,
                    TargetCurrencyId = targetCurrency.Id,
                    Date = date.Date,
                    Rate = rate.Value
                };


                await _repositoryWrapper.CurrencyRepository.AddExchangeRateAsync(newRate);

                if (date.Date == DateTime.Today.Date)
                {
                    targetCurrency.ExchangeRate = rate.Value;
                }

                await _repositoryWrapper.Save();
                return newRate.Rate;
            }

            // Fallback: get most recent rate before the requested date
            var lastKnownRate = await _repositoryWrapper.CurrencyRepository.GetLatestRateBeforeDateAsync(baseCurrency.Id, targetCurrency.Id, date);

            if (lastKnownRate != null)
                return lastKnownRate.Rate;

            throw new Exception("No exchange rate available for the requested or previous dates.");
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

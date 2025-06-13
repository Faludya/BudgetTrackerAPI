using Services.Interfaces;
using System.Net.Http.Json;

namespace Services
{
    public class FrankfurterApiService : ICurrencyApiService
    {
        private readonly HttpClient _httpClient;

        public FrankfurterApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal?> GetRateAsync(string targetCurrencyCode, DateTime date)
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            var url = $"https://api.frankfurter.app/{formattedDate}?from=EUR&to={targetCurrencyCode}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<FrankfurterResponse>(url);
                if (response?.Rates.TryGetValue(targetCurrencyCode, out var rate) == true)
                    return rate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Frankfurter API error: {ex.Message}");
            }

            return null;
        }

        private class FrankfurterResponse
        {
            public Dictionary<string, decimal> Rates { get; set; } = new();
        }
    }

}

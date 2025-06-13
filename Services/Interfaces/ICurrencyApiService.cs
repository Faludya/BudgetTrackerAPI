namespace Services.Interfaces
{
    public interface ICurrencyApiService
    {
        Task<decimal?> GetRateAsync(string targetCurrencyCode, DateTime date);
    }
}

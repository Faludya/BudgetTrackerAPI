namespace Models
{
    public class CurrencyExchangeRate
    {
        public int Id { get; set; }

        // Foreign key to base currency (EUR in your case)
        public int BaseCurrencyId { get; set; }
        public Currency BaseCurrency { get; set; }

        // Foreign key to target currency (e.g., RON, USD)
        public int TargetCurrencyId { get; set; }
        public Currency TargetCurrency { get; set; }

        public DateTime Date { get; set; }
        public decimal Rate { get; set; }
    }

}

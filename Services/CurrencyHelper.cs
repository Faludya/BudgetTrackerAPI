using Models;

namespace Services
{
    public class CurrencyHelper
    {
        public static decimal ConvertAmount(decimal amount, Currency from, Currency to)
        {
            if (from.Code == to.Code || from.ExchangeRate == 0)
                return amount;

            var baseAmount = amount / from.ExchangeRate;
            return Math.Round(baseAmount * to.ExchangeRate, 2);
        }
    }

}

using Models;

namespace Repositories.Interfaces
{
    public interface ICurrencyRepository : IRepositoryBase<Currency>
    {
        Task<Currency> GetCurrencyById(int id);
        Task<Currency> GetCurrencyByCode(string code);
    }
}

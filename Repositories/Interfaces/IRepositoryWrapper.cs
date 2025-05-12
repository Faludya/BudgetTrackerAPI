namespace Repositories.Interfaces
{
    public interface IRepositoryWrapper
    {
        IApplicationUserRepository ApplicationUserRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        ICurrencyRepository CurrencyRepository { get; }
        IUserPreferencesRepository UserPreferencesRepository { get; }
        Task Save();
    }
}

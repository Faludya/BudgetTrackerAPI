namespace Repositories.Interfaces
{
    public interface IRepositoryWrapper
    {
        IApplicationUserRepository ApplicationUserRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        ICurrencyRepository CurrencyRepository { get; }
        IUserPreferencesRepository UserPreferencesRepository { get; }
        IImportSessionRepository ImportSessionRepository { get; }
        IImportedTransactionRepository ImportedTransactionRepository { get; }
        ICategorySuggestionRepository CategorySuggestionRepository { get; }
        Task Save();
    }
}

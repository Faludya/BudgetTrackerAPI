using Models;

namespace Repositories.Interfaces
{
    public interface IUserPreferencesRepository : IRepositoryBase<UserPreferences>
    {
        Task<UserPreferences> GetUserPreferences(string userId);
    }
}

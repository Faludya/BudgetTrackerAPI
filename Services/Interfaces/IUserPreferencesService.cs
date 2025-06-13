using Models;

namespace Services.Interfaces
{
    public interface IUserPreferencesService
    {
        Task<UserPreferences> GetUserPreferencesByIdAsync(string userId);
        Task AddUserPreferencesAsync(UserPreferences userPreferences);
        Task UpdateUserPreferencesAsync(UserPreferences userPreferences);
        Task DeleteUserPreferencesAsync(string userId);
        Task CreateDefaultUserPreferences(string userId);
    }
}

using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class UserPreferencesService : IUserPreferencesService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public UserPreferencesService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task AddUserPreferencesAsync(UserPreferences userPreferences)
        {
            if (userPreferences == null)
            {
                throw new ArgumentNullException(nameof(userPreferences), "User preference cannot be null.");
            }

            await _repositoryWrapper.UserPreferencesRepository.Create(userPreferences);
            await _repositoryWrapper.Save();
        }

        public async Task DeleteUserPreferencesAsync(string userId)
        {
            var preference = await _repositoryWrapper.UserPreferencesRepository.GetUserPreferences(userId);
            if (preference == null)
            {
                throw new KeyNotFoundException($"Category with ID {userId} not found.");
            }

            await _repositoryWrapper.UserPreferencesRepository.Delete(preference);
            await _repositoryWrapper.Save();
        }

        public async Task<UserPreferences> GetUserPreferencesByIdAsync(string userId)
        {
            return await _repositoryWrapper.UserPreferencesRepository.GetUserPreferences(userId);
        }

        public async Task UpdateUserPreferencesAsync(UserPreferences userPreferences)
        {
            var existing = await _repositoryWrapper.UserPreferencesRepository.GetUserPreferences(userPreferences.UserId);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Preferences not found for user {userPreferences.UserId}.");
            }

            existing.PreferredCurrency = userPreferences.PreferredCurrency;
            existing.Theme = userPreferences.Theme;
            existing.DateFormat = userPreferences.DateFormat;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repositoryWrapper.UserPreferencesRepository.Update(existing);
            await _repositoryWrapper.Save();
        }
    }
}

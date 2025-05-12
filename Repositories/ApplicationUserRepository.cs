using System.Security.Cryptography.Pkcs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class ApplicationUserRepository : IApplicationUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserPreferencesRepository _userPreferencesRepository;
        public ApplicationUserRepository(UserManager<ApplicationUser> userManager, IUserPreferencesRepository userPreferencesRepository)
        {
            _userManager = userManager;
            _userPreferencesRepository = userPreferencesRepository;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return _userManager.Users.AsEnumerable();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string? password)
        {
            IdentityResult result;

            if (string.IsNullOrEmpty(password))
            {
                result = await _userManager.CreateAsync(user);
            }
            else
            {
                result = await _userManager.CreateAsync(user, password);
            }

            if (result.Succeeded)
            {
                var defaultPrefs = new UserPreferences
                {
                    UserId = user.Id,
                };

                await _userPreferencesRepository.Create(defaultPrefs);
            }

            return result;
        }



        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(ApplicationUser user)
        {
            return await _userManager.DeleteAsync(user);
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }
    }

}

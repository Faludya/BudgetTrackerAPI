using Microsoft.AspNetCore.Identity;
using Models;

namespace Services.Interfaces
{
    public interface IApplicationUserService
    {
        string GenerateJwtToken(ApplicationUser user);

        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<IdentityResult> RegisterUserAsync(ApplicationUser user, string password);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<bool> ValidateUserAsync(string email, string password);
    }

}

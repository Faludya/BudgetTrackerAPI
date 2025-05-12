using Microsoft.AspNetCore.Mvc;

namespace BudgetTrackerAPI.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Services.Interfaces;

    namespace BudgetTrackerAPI.Controllers
    {
        [Authorize]
        [ApiController]
        [Route("api/[controller]")]
        [ValidateUserIdHeader]
        public class UserPreferencesController : ControllerBase
        {
            private readonly IUserPreferencesService _preferencesService;

            public UserPreferencesController(IUserPreferencesService preferencesService)
            {
                _preferencesService = preferencesService;
            }
         
            [HttpGet]
            public async Task<IActionResult> GetPreferences()
            {
                if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not string userId)
                    return BadRequest("UserId not found in context.");

                var preferences = await _preferencesService.GetUserPreferencesByIdAsync(userId);
                return preferences != null ? Ok(preferences) : NotFound();
            }

            [HttpPut]
            public async Task<IActionResult> UpdatePreferences([FromBody] UserPreferences updatedPrefs)
            {
                if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not string userId)
                    return BadRequest("UserId not found in context.");

                if (userId != updatedPrefs.UserId)
                    return BadRequest("User ID mismatch in request body.");

                try
                {
                    await _preferencesService.UpdateUserPreferencesAsync(updatedPrefs);
                    return NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
            }
        }
    }

}

using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;

namespace BudgetTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/userbudgets")]
    public class UserBudgetController : ControllerBase
    {
        private readonly IUserBudgetService _userBudgetService;

        public UserBudgetController(IUserBudgetService userBudgetService)
        {
            _userBudgetService = userBudgetService;
        }

        [HttpGet("{userId}/{month}/{year}")]
        public async Task<IActionResult> GetUserBudget(string userId, int month, int year)
        {
            var budget = await _userBudgetService.GetBudgetForMonthAsync(userId, month, year);
            return budget == null ? NotFound() : Ok(budget);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserBudget([FromBody] UserBudget userBudget)
        {
            await _userBudgetService.AddBudgetAsync(userBudget);
            return CreatedAtAction(nameof(GetUserBudget), new { userId = userBudget.UserId, month = userBudget.Month.Month, year = userBudget.Month.Year }, userBudget);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserBudget(int id, [FromBody] UserBudget userBudget)
        {
            if (id != userBudget.Id) return BadRequest();
            await _userBudgetService.UpdateBudgetAsync(userBudget);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserBudget(int id)
        {
            await _userBudgetService.DeleteBudgetAsync(id);
            return NoContent();
        }
    }


}

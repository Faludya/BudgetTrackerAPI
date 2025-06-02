using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;
using Services.Interfaces;

namespace BudgetTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/userbudgets")]
    public class UserBudgetController : ControllerBase
    {
        private readonly IUserBudgetService _userBudgetService;
        private readonly IUserBudgetItemService _userBudgetItemService;

        public UserBudgetController(IUserBudgetService userBudgetService, IUserBudgetItemService userBudgetItemService)
        {
            _userBudgetService = userBudgetService;
            _userBudgetItemService = userBudgetItemService;
        }

        [HttpGet("{userId}/{month}/{year}")]
        public async Task<IActionResult> GetUserBudget(string userId, int month, int year)
        {
            var budget = await _userBudgetService.GetBudgetForMonthAsync(userId, month, year);
            return budget == null ? NotFound() : Ok(budget);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateBudgetFromTemplate([FromBody] GenerateBudgetRequest request)
        {
            var budget = await _userBudgetService.CreateBudgetFromTemplateAsync(
                request.UserId, request.Month, request.Year, request.TemplateId, request.Income
            );
            return Ok(budget);
        }

        [HttpPost("category-limit")]
        public async Task<IActionResult> AddOrUpdateCategoryLimit([FromBody] CategoryLimitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _userBudgetService.AddOrUpdateCategoryLimitAsync(dto);
            return updated != null ? Ok(updated) : BadRequest("Could not save category limit.");
        }

        [HttpDelete("category-limit/{id}")]
        public async Task<IActionResult> DeleteCategoryLimit(int id)
        {
            await _userBudgetItemService.DeleteItemAsync(id);
            return NoContent();
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

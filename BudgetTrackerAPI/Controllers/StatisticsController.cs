using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace BudgetTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromHeader] string userId, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _statisticsService.GetSummaryAsync(userId, month, year);
            return Ok(result);
        }

        [HttpGet("top-categories")]
        public async Task<IActionResult> GetTopCategories([FromHeader] string userId, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _statisticsService.GetTopCategoriesAsync(userId, month, year);
            return Ok(result);
        }

        [HttpGet("monthly-expenses")]
        public async Task<IActionResult> GetMonthlyExpenses([FromHeader] string userId)
        {
            var result = await _statisticsService.GetMonthlyExpensesAsync(userId);
            return Ok(result);
        }

        [HttpGet("budget-vs-actual")]
        public async Task<IActionResult> GetBudgetVsActual([FromHeader] string userId, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _statisticsService.GetBudgetVsActualAsync(userId, month, year);
            return Ok(result);
        }

        [HttpGet("budget-category-usage")]
        public async Task<IActionResult> GetBudgetCategoryUsage([FromHeader] string userId, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _statisticsService.GetBudgetUsageByCategoryAsync(userId, month, year);
            return Ok(result);
        }

        [HttpGet("overspent-categories")]
        public async Task<IActionResult> GetOverspentCategories([FromHeader] string userId, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _statisticsService.GetOverspentCategoryNamesAsync(userId, month, year);
            return Ok(result);
        }

        [HttpGet("budget-health-summary")]
        public async Task<IActionResult> GetBudgetHealthSummary([FromHeader] string userId, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _statisticsService.GetBudgetHealthSummaryAsync(userId, month, year);
            return Ok(result);
        }
    }

}
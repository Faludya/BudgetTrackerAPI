using Microsoft.AspNetCore.Mvc;
using Models.DTOs;
using Services.Interfaces;

namespace BudgetTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/dashboard-layout")]
    public class DashboardLayoutController : ControllerBase
    {
        private readonly IDashboardLayoutService _dashboardLayoutService;

        public DashboardLayoutController(IDashboardLayoutService dashboardLayoutService)
        {
            _dashboardLayoutService = dashboardLayoutService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLayout([FromHeader] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            var layout = await _dashboardLayoutService.GetLayoutAsync(userId);
            return Ok(layout);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLayout([FromHeader] string userId, [FromBody] DashboardLayoutDto layoutDto)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");


            await _dashboardLayoutService.SaveLayoutAsync(userId, layoutDto.WidgetOrder);
            return Ok(new { success = true });
        }
    }
}

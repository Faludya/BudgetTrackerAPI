using Microsoft.AspNetCore.Mvc;
using Models.DTOs;
using Services.Interfaces;

namespace BudgetTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ValidateUserIdHeader]
    public class CategoryKeywordMappingController : ControllerBase
    {
        private readonly ICategoryKeywordMappingService _mappingService;

        public CategoryKeywordMappingController(ICategoryKeywordMappingService mappingService)
        {
            _mappingService = mappingService;
        }

        /// <summary>
        /// Returns all keyword mappings for the logged-in user.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not string userId)
                return BadRequest("UserId not found in context.");

            var mappings = await _mappingService.GetAllForUserAsync(userId);

            var result = mappings.Select(m => new
            {
                m.Id,
                m.Keyword,
                m.CategoryId,
                CategoryName = m.Category.Name,
                m.CreatedAt
            });

            return Ok(result);
        }

        /// <summary>
        /// Creates a new keyword mapping.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryKeywordMappingDto dto)
        {
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not string userId)
                return BadRequest("UserId not found in context.");

            try
            {
                await _mappingService.AddAsync(userId, dto);
                return Ok(new { message = "Mapping saved successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing keyword mapping.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCategoryKeywordMappingDto dto)
        {
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not string userId)
                return BadRequest("UserId not found in context.");

            var success = await _mappingService.UpdateAsync(userId, dto);
            if (!success)
                return NotFound(new { message = "Mapping not found or not owned by user." });

            return Ok(new { message = "Mapping updated successfully." });
        }

        /// <summary>
        /// Deletes a keyword mapping.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not string userId)
                return BadRequest("UserId not found in context.");

            var success = await _mappingService.DeleteAsync(userId, id);
            if (!success)
                return NotFound(new { message = "Mapping not found or not owned by user." });

            return NoContent();
        }
    }
}
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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all categories.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not string userId)
            {
                return BadRequest("UserId not found in context.");
            }

            var categories = await _categoryService.GetAllCategoriesAsync(userId);
            return Ok(categories);
        }

        /// <summary>
        /// Get a category by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new category.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (category == null)
                return BadRequest(new { message = "Category cannot be null" });

            await _categoryService.AddCategoryAsync(category);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        /// <summary>
        /// Update an existing category.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category category)
        {
            if (id != category.Id)
                return BadRequest(new { message = "Category ID mismatch" });

            try
            {
                await _categoryService.UpdateCategoryAsync(category);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a category.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }


}

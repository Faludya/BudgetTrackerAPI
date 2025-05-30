using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;

namespace BudgetTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/budget-templates")]
    public class BudgetTemplateController : ControllerBase
    {
        private readonly IBudgetTemplateService _budgetTemplateService;

        public BudgetTemplateController(IBudgetTemplateService budgetTemplateService)
        {
            _budgetTemplateService = budgetTemplateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _budgetTemplateService.GetAllTemplatesAsync();
            return Ok(templates);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplate(int id)
        {
            var template = await _budgetTemplateService.GetTemplateByIdAsync(id);
            if (template == null) return NotFound();
            return Ok(template);
        }

        [HttpPost]
        public async Task<IActionResult> AddTemplate([FromBody] BudgetTemplate template)
        {
            await _budgetTemplateService.AddTemplateAsync(template);
            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] BudgetTemplate template)
        {
            if (id != template.Id) return BadRequest();
            await _budgetTemplateService.UpdateTemplateAsync(template);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            await _budgetTemplateService.DeleteTemplateAsync(id);
            return NoContent();
        }
    }

}

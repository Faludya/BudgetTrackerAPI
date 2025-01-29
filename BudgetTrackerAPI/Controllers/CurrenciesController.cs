using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;

namespace BudgetTrackerAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrenciesController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var currencies = await _currencyService.GetAllCurrenciesAsync();
            return Ok(currencies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var currency = await _currencyService.GetCurrencyByIdAsync(id);
                return Ok(currency);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Currency currency)
        {
            await _currencyService.AddCurrencyAsync(currency);
            return CreatedAtAction(nameof(GetById), new { id = currency.Id }, currency);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Currency currency)
        {
            if (id != currency.Id)
            {
                return BadRequest("Currency ID mismatch.");
            }

            try
            {
                await _currencyService.UpdateCurrencyAsync(currency);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _currencyService.DeleteCurrencyAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }

}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;
using Services.Interfaces;

namespace BudgetTrackerAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [ValidateUserIdHeader]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not string userId)
            {
                return BadRequest("UserId not found in context.");
            }

            var transactions = await _transactionService.GetAllTransactionsAsync(userId);
            return Ok(transactions);
        }

        [HttpGet("filtered")]
        public async Task<IActionResult> GetFiltered([FromQuery] TransactionFilterDto filters)
        {
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not string userId)
            {
                return BadRequest("UserId not found in context.");
            }

            var result = await _transactionService.GetAllFilteredTransactions(userId, filters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                return Ok(transaction);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            await _transactionService.AddTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return BadRequest("Transaction ID mismatch.");
            }

            try
            {
                await _transactionService.UpdateTransactionAsync(transaction);
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
                await _transactionService.DeleteTransactionAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] TransactionFilterDto filters, [FromQuery] string format)
        {
            try
            {
                if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not string userId)
                {
                    return BadRequest("UserId not found in context.");
                }

                filters.FromDate = filters.FromDate?.ToUniversalTime();
                filters.ToDate = filters.ToDate?.ToUniversalTime();

                var result = await _transactionService.ExportAsync(userId, filters, format.ToLower());

                if (result == null)
                    return BadRequest("Unsupported export format.");

                var contentType = format.ToLower() == "excel"
                    ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    : "application/pdf";

                var fileName = $"transactions_{DateTime.Now:yyyyMMdd_HHmm}.{(format == "excel" ? "xlsx" : "pdf")}";

                return File(result.Content, contentType, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"Export failed: {ex.Message}");
            }
        }
    }

}

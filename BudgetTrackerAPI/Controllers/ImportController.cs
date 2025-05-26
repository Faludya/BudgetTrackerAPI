using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs;
using Services;
using Services.Interfaces;
using System.Security.Claims;

namespace BudgetTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/import")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;
        private readonly IImportSessionService _importSessionService;
        private readonly IImportedTransactionService _importedTransactionService;

        public ImportController(IImportService importService, IImportSessionService importSessionService, IImportedTransactionService importedTransactionService)
        {
            _importService = importService;
            _importSessionService = importSessionService;
            _importedTransactionService = importedTransactionService;
        }

        [HttpPost("start-session")]
        public async Task<IActionResult> StartImportSession([FromForm] IFormFile file, [FromForm] string template)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? HttpContext.Request.Headers["userId"].ToString();

            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (file == null || file.Length == 0) return BadRequest("Invalid file.");

            var session = await _importService.CreateImportSessionAsync(file, template, userId);
            return Ok(new
            {
                session.Id,
                session.CreatedAt,
                session.Template,
                Transactions = session.Transactions
            });
        }

        [HttpGet("session/{id}")]
        public async Task<IActionResult> GetImportSession(Guid id)
        {
            var session = await _importService.GetImportSessionAsync(id);
            return session == null ? NotFound() : Ok(session);
        }

        [HttpGet("session/in-progress")]
        public async Task<IActionResult> GetInProgressSession()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? HttpContext.Request.Headers["userId"].ToString();

            var session = await _importSessionService.GetImportSessionByUserIdAsync(userId);
            return session == null ? NotFound() : Ok(session);
        }


        [HttpPut("session/{sessionId}/transaction/{transactionId}")]
        public async Task<IActionResult> UpdateImportedTransaction(Guid sessionId, int transactionId, [FromBody] UpdateImportedTransactionDto dto)
        {
            var success = await _importService.UpdateImportedTransactionAsync(sessionId, transactionId, dto);
            return success ? Ok() : NotFound();
        }

        [HttpDelete("transaction/{transactionId}")]
        public async Task<IActionResult> DeleteImportedTransaction(Guid sessionId, int transactionId)
        {
            try
            {
                await _importedTransactionService.DeleteImportedTransactionAsync(transactionId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("session/{sessionId}")]
        public async Task<IActionResult> CancelImportSession(Guid sessionId)
        {
            try
            {
                await _importSessionService.DeleteImportSessionAsync(sessionId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("session/{id}/complete")]
        public async Task<IActionResult> CompleteImport(Guid id)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? HttpContext.Request.Headers["userId"].ToString();

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _importService.CompleteImportAsync(id, userId);
            return Ok(result);
        }
    }
}

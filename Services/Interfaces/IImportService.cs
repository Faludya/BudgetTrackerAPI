using Microsoft.AspNetCore.Http;
using Models;
using Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IImportService
    {
        Task<ImportSession?> GetImportSessionAsync(Guid id);
        Task<string> CompleteImportAsync(Guid id, string userId);
        Task<ImportSession> CreateImportSessionAsync(IFormFile file, string template, string userId);
        Task<bool> UpdateImportedTransactionAsync(Guid sessionId, int transactionId, UpdateImportedTransactionDto dto);
        Task<List<ImportSession>> GetAllSessionsForUserAsync(string userId);

    }
}

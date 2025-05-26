using Models;

namespace Services.Interfaces
{
    public interface IImportSessionService
    {
        Task<ImportSession> GetImportSessionByIdAsync(Guid id);
        Task AddImportSessionAsync(ImportSession importSession);
        Task UpdateImportSessionAsync(ImportSession importSession);
        Task DeleteImportSessionAsync(Guid id);
    }
}

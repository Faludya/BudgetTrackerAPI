using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class ImportSessionService : IImportSessionService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public ImportSessionService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task AddImportSessionAsync(ImportSession importSession)
        {
            if (importSession == null)
            {
                throw new ArgumentNullException(nameof(importSession), "Imported session cannot be null.");
            }

            await _repositoryWrapper.ImportSessionRepository.Create(importSession);
            await _repositoryWrapper.Save();
        }

        public async Task DeleteImportSessionAsync(Guid id)
        {
            var importSession = await _repositoryWrapper.ImportSessionRepository.GetImportSessionById(id);
            if (importSession == null)
            {
                throw new KeyNotFoundException($"Imported session  with ID {id} not found.");
            }

            await _repositoryWrapper.ImportSessionRepository.Delete(importSession);
            await _repositoryWrapper.Save();
        }

        public async Task<ImportSession> GetImportSessionByIdAsync(Guid id)
        {
            return await _repositoryWrapper.ImportSessionRepository.GetImportSessionById(id)
                ?? throw new KeyNotFoundException($"Import session with ID {id} not found.");
        }

        public async Task UpdateImportSessionAsync(ImportSession importSession)
        {
            if (importSession == null)
            {
                throw new ArgumentNullException(nameof(importSession), "Imported session cannot be null.");
            }

            var existingImportSession = await _repositoryWrapper.ImportSessionRepository.GetImportSessionById(importSession.Id);
            if (existingImportSession == null)
            {
                throw new KeyNotFoundException($"Category with ID {importSession.Id} not found.");
            }

            await _repositoryWrapper.ImportSessionRepository.Update(existingImportSession);
            await _repositoryWrapper.Save();
        }
    }
}

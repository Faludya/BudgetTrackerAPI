using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class ImportedTransactionService : IImportedTransactionService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public ImportedTransactionService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task AddImportedTransactionAsync(ImportedTransaction importedTransaction)
        {
            if (importedTransaction == null)
            {
                throw new ArgumentNullException(nameof(importedTransaction), "Imported Transaction cannot be null.");
            }

            await _repositoryWrapper.ImportedTransactionRepository.Create(importedTransaction);
            await _repositoryWrapper.Save();
        }

        public async Task DeleteImportedTransactionAsync(int id)
        {
            var importedTransaction = await _repositoryWrapper.ImportedTransactionRepository.GetImportedTransactionById(id);
            if (importedTransaction == null)
            {
                throw new KeyNotFoundException($"Imported transaction  with ID {id} not found.");
            }

            await _repositoryWrapper.ImportedTransactionRepository.Delete(importedTransaction);
            await _repositoryWrapper.Save();
        }

        public async Task<ImportedTransaction> GetImportedTransactionByIdAsync(int id)
        {
            return await _repositoryWrapper.ImportedTransactionRepository.GetImportedTransactionById(id)
                ?? throw new KeyNotFoundException($"Import transaction with ID {id} not found.");
        }

        public async Task UpdateImportedTransactionAsync(ImportedTransaction importedTransaction)
        {
            if (importedTransaction == null)
            {
                throw new ArgumentNullException(nameof(importedTransaction), "Imported transaction cannot be null.");
            }

            var existingImportedTransaction = await _repositoryWrapper.ImportedTransactionRepository.GetImportedTransactionById(importedTransaction.Id);
            if (existingImportedTransaction == null)
            {
                throw new KeyNotFoundException($"Category with ID {importedTransaction.Id} not found.");
            }

            await _repositoryWrapper.ImportedTransactionRepository.Update(existingImportedTransaction);
            await _repositoryWrapper.Save();
        }
    }
}

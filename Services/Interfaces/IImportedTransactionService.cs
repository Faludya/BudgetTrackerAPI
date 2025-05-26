using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IImportedTransactionService
    {
        Task<ImportedTransaction> GetImportedTransactionByIdAsync(int id);
        Task AddImportedTransactionAsync(ImportedTransaction importedTransaction);
        Task UpdateImportedTransactionAsync(ImportedTransaction importedTransaction);
        Task DeleteImportedTransactionAsync(int id);
    }
}

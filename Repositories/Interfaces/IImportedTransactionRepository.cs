using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IImportedTransactionRepository : IRepositoryBase<ImportedTransaction>
    {
        Task<ImportedTransaction?> GetImportedTransactionById(int id);
        Task<ImportedTransaction?> GetByIdAndSessionAsync(int transactionId, Guid sessionId);
    }
}

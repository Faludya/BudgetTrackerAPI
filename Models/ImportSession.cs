using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ImportSession
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Template { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsCompleted { get; set; } = false;

        public ICollection<ImportedTransaction> Transactions { get; set; } = new List<ImportedTransaction>();
    }

}

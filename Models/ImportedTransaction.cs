using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ImportedTransaction
    {
        public int Id { get; set; }
        public Guid ImportSessionId { get; set; }
        public ImportSession ImportSession { get; set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string? Category { get; set; }
        public bool RememberMapping { get; set; } = false;
        public bool IsFromMLModel { get; set; } = false;
    }

}

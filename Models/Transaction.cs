using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date {  get; set; }
        public DateTime CreatedAt { get; set; }

        //Foreign Keys
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }
    }
}

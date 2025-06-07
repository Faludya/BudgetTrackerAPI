using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class UserBudget
    {
        public int Id { get; set; }

        public string UserId { get; set; } = default!;
        public DateTime Month { get; set; }
        public string CurrencyCode { get; set; } = "EUR";

        public ICollection<UserBudgetItem> BudgetItems { get; set; } = new List<UserBudgetItem>();
    }

}

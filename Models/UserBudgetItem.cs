using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class UserBudgetItem
    {
        public int Id { get; set; }

        public int UserBudgetId { get; set; }
        public UserBudget UserBudget { get; set; } = default!;

        public int? CategoryId { get; set; } // Can be null if it's a category type like "Needs"
        public Category? Category { get; set; } 
        public string? CategoryType { get; set; } // optional fallback (for Needs/Wants/Savings breakdown)

        public decimal Limit { get; set; }

        // Optional fields for future insights
        public decimal? PredictedSpending { get; set; }
        public string? TrendDirection { get; set; } // e.g., "increasing", "stable"
        public bool IsAIRecommended { get; set; } = false;
        [NotMapped]
        public decimal ConvertedLimit { get; set; }
    }

}

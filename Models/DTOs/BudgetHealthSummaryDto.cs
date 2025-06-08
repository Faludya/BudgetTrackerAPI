namespace Models.DTOs
{
    public class BudgetHealthSummaryDto
    {
        public int TotalCategories { get; set; }
        public int WithinBudgetCount { get; set; }
        public int OverspentCount => TotalCategories - WithinBudgetCount;
        public decimal AverageUsagePercent { get; set; }
    }
}

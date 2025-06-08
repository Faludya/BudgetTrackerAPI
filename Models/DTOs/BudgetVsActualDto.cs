namespace Models.DTOs
{
    public class BudgetVsActualDto
    {
        public decimal TotalBudgeted { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal Remaining => TotalBudgeted - TotalSpent;
        public decimal PercentageUsed => TotalBudgeted == 0 ? 0 : (TotalSpent / TotalBudgeted) * 100;
    }
}

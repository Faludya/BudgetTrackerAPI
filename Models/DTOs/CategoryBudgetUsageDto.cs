namespace Models.DTOs
{
    public class CategoryBudgetUsageDto
    {
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal Limit { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining => Limit - Spent;
        public decimal PercentageUsed => Limit == 0 ? 0 : (Spent / Limit) * 100;
        public bool IsOverLimit => Spent > Limit;
    }
}

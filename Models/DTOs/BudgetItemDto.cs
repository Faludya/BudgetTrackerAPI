namespace Models.DTOs
{
    public class BudgetItemDto
    {
        public int? CategoryId { get; set; }
        public string CategoryType { get; set; } = default!;
        public decimal Limit { get; set; }
    }

}

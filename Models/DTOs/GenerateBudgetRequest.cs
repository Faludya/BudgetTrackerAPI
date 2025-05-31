namespace Models.DTOs
{
    public class GenerateBudgetRequest
    {
        public string UserId { get; set; } = default!;
        public int Month { get; set; }
        public int Year { get; set; }
        public int TemplateId { get; set; }
        public decimal Income { get; set; }
    }

    public class CategoryLimitRequest
    {
        public string UserId { get; set; } = default!;
        public int Month { get; set; }
        public int Year { get; set; }
        public int CategoryId { get; set; }
        public decimal Limit { get; set; }
    }
}

namespace Models.DTOs
{
    public class BudgetTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public List<BudgetTemplateItemDto> Items { get; set; } = new();
    }

    public class BudgetTemplateItemDto
    {
        public string CategoryType { get; set; } = default!;
        public decimal Percentage { get; set; }
    }

}

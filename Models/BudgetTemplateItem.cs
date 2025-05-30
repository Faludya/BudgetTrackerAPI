namespace Models
{
    public class BudgetTemplateItem
    {
        public int Id { get; set; }

        public int BudgetTemplateId { get; set; }
        public BudgetTemplate BudgetTemplate { get; set; } = default!;

        public string CategoryType { get; set; } = default!; // e.g. "Needs", "Wants"
        public decimal Percentage { get; set; }
    }

}

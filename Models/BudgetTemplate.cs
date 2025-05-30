namespace Models
{
    public class BudgetTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public ICollection<BudgetTemplateItem> Items { get; set; } = new List<BudgetTemplateItem>();
    }

}

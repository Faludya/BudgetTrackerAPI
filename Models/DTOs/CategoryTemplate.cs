namespace Models.DTOs
{
    public class CategoryTemplate
    {
        public string Name { get; set; } = default!;
        public string? ColorHex { get; set; }
        public string? IconName { get; set; }
        public int OrderIndex { get; set; }
        public string? CategoryType { get; set; }
        public List<SubCategoryTemplate>? Subcategories { get; set; }
    }

    public class SubCategoryTemplate
    {
        public string Name { get; set; } = default!;
    }

}

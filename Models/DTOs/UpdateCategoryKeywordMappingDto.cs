namespace Models.DTOs
{
    public class UpdateCategoryKeywordMappingDto
    {
        public int Id { get; set; }
        public string Keyword { get; set; } = default!;
        public int CategoryId { get; set; }
    }

}

namespace Models.DTOs
{
    public class CategoryKeywordMappingDto
    {
        public string Keyword { get; set; } = default!;
        public int CategoryId { get; set; }
    }

}

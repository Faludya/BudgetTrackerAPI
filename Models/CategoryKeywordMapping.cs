namespace Models
{
    public class CategoryKeywordMapping
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public string Keyword { get; set; } = default!;
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Category Category { get; set; } = default!;
    }

}

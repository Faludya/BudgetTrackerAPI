namespace Models
{
    public class CategorySuggestion
    {
        public int Id { get; set; }

        public int? ImportedTransactionId { get; set; }
        public ImportedTransaction? ImportedTransaction { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        public decimal Confidence { get; set; } // range 0.0 - 1.0
        public bool IsFromMLModel { get; set; } = false;
        public string? SourceKeyword { get; set; } 

        public DateTime SuggestedAt { get; set; } = DateTime.UtcNow;
    }

}

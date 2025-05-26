namespace Models.DTOs
{
    public class ParsedTransactionDto
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public string? Category { get; set; } // Optional category name suggestion
    }

}

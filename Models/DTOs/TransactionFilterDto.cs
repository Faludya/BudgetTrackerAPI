namespace Models.DTOs
{
    public class TransactionFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }
    }
}

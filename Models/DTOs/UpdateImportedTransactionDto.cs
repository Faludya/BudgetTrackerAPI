﻿namespace Models.DTOs
{
    public class UpdateImportedTransactionDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public string? Category { get; set; }
        public bool RememberMapping { get; set; }
        public bool IsFromMLModel { get; set; }
    }
}

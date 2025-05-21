using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; } 

        public int CategoryId { get; set; }
        public int CurrencyId { get; set; }
        [NotMapped]
        public decimal? ConvertedAmount { get; set; }

        //These navigation properties should be optional to prevent validation errors
        public virtual ApplicationUser? User { get; set; }
        public virtual Category? Category { get; set; }
        public virtual Currency? Currency { get; set; }
    }

}

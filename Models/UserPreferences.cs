using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class UserPreferences
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }  

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }   

        [MaxLength(3)]
        public string PreferredCurrency { get; set; } = "EUR";

        [MaxLength(10)]
        public string Theme { get; set; } = "light"; // light or dark

        [MaxLength(20)]
        public string DateFormat { get; set; } = "DD/MM/YYYY";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

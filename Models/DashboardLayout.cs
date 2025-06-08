using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class DashboardLayout
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = default!;
        [Column(TypeName = "jsonb")]
        public List<string> WidgetOrder { get; set; } = new();
    }

}

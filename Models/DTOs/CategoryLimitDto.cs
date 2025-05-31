using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class CategoryLimitDto
    {
        [Required]
        public string UserId { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        [Range(2000, 3000)]
        public int Year { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Limit { get; set; }
        [Required]
        public string ParentCategoryType { get; set; }
    }

}

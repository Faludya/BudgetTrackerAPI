using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public bool IsPredefined { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CategoryType { get; set; } // "Essentials", "Wants", etc.

        //Foreign Keys
        public int? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category>? SubCategories { get; set; }


        public string UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }




    }
}

using Microsoft.AspNetCore.Identity;

namespace Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? GoogleId { get; set; }
        //Personal information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth {  get; set; }
        public string? PhoneNumber {  get; set; }
        public string? Address { get; set; }
        public string? ProfilePictureUrl { get; set; }
       
        //Updates history
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

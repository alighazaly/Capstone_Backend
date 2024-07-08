using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class AppUser : IdentityUser
    {

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string? ProfilePicture { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        [NotMapped]
        public string ImageSrc { get; set; }

        [Required]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Please enter a valid email address")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }

        public String Role { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match!")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public ICollection<Appartment>? Appartments { get; set; }

        public ICollection<Review>? Reviews { get; set; }

        public ICollection<Reservation>? Reservations { get; set; }

        public ICollection<Feedback>? Feedbacks { get; set; }

        public ICollection<Requests>? requests { get; set; }

        public ICollection<Responses>? responses {  get; set; }

        public int WishListId { get; set; }
        [ForeignKey("WishListId")]
        public WishList list { get; set; }

    }
}

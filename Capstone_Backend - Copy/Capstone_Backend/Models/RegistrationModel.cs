using System.ComponentModel.DataAnnotations;

namespace Capstone_Server.Models
{
    public class RegistrationModel
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(15,MinimumLength = 5,ErrorMessage = "UserName must be at least 5 characters long.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password don't match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}


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

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and include an uppercase letter, a lowercase letter, a number, and a special character.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password don't match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}


using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string ProfilePicture { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string ImageSrc { get; set; }

    }
}

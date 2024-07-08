using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int Value { get; set; }
        [Required]
        public String Content { get; set; }

        public string WriterId { get; set; }

        // Navigation property with a meaningful name
        [ForeignKey("WriterId")]
        public AppUser Writer { get; set; }


    }
}

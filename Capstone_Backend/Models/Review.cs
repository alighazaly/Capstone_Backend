using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public double Value { get; set; }

        public DateTime DateRated { get; set; }

        public string ReviewerId { get; set; }

        [ForeignKey("ReviewerId")] // Explicit foreign key attribute
        public AppUser Reviewer { get; set; }

        [ForeignKey("AppartmentId")]
        public Appartment Appartment { get; set; }
        public int AppartmentId { get; set; }
        public bool HasReservedBefore { get; set; }
    }
}

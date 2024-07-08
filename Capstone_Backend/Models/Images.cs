using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class Images
    {
        public int Id { get; set; }
        public string Url { get; set; }

        // Foreign key
        public int AppartmentId { get; set; }

        [ForeignKey("AppartmentId")]
        public Appartment Appartment { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }
    }
}

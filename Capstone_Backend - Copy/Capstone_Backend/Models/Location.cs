using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        public ICollection<Appartment>? Appartments { get; set; }

 
    }
}

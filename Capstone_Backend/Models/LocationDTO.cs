using System.ComponentModel.DataAnnotations;

namespace Capstone_Backend.Models
{
    public class LocationDTO
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}

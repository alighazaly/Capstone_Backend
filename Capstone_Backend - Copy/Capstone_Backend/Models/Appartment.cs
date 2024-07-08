using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class Appartment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public String Title { get; set; }
        [Required]
        public String Description { get; set; }
        public DateTime UploadDate { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public int NumberOfBedrooms { get; set; }
        [Required]
        public int NumberOfBathrooms { get; set; }
        [Required]
        public int NumberOfBeds { get; set; }
        [Required]
        public String Elevator { get; set; }
        [Required]
        public String Generator { get; set; }
        [Required]
        public double Area { get; set; }
        [Required]
        public int MasterBedrooms { get; set; }
        [Required]
        public String Garden { get; set; }
        [Required]
        public int WaterContainers { get; set; }
        [Required]
        public String Pool { get; set; }
        [Required]
        public String Guard { get; set; }
        [Required]
        public String Kitchen { get; set; }
        [Required]
        public String BbqGrill { get; set; }
        [Required]
        public String HotTube { get; set; }
        [Required]
        public String Wifi { get; set; }
        [Required]
        public String WorkSpace { get; set; }
        [Required]
        public String IndoorFirePlace { get; set; }
        [Required]
        public String SmokingAllowed { get; set; }
        [Required]
        public String Gym { get; set; }
        [Required]
        public int Tvs { get; set; }
        [Required]
        public int Parking { get; set; }
        [Required]
        public String airConditionner { get; set; }
        [Required]
        public String TypeOfPlace { get; set; }
        public string OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public AppUser Owner { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public Category Category { get; set; }
        public int LocationId { get; set; }
        [ForeignKey("LocationId")]

        public Location Location { get; set; }

        public ICollection<Review>? Reviews { get; set; }

        public ICollection<Reservation>? Reservations { get; set; }

   
        public ICollection<AppartmentWishList>? Wishlist { get; set; }

        public ICollection<Requests>? requests {  get; set; }

        public List<Images> images { get; set; }

        public Appartment()
        {
            Reviews = new List<Review>();
            Reservations = new List<Reservation>();
            requests = new List<Requests>();
            UploadDate = DateTime.Now;
        }

    }
}

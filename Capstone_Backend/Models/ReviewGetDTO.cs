namespace Capstone_Backend.Models
{
    public class ReviewGetDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public double Value { get; set; }
        public DateTime DateRated { get; set; }
        public int AppartmentId { get; set; }
        public String ReviewrFirstName { get; set; }
        public String ReviewrLastName { get; set; }
        public string ProfilePicture { get; set; }
        public string ImageSrc { get; set; }
        public bool HasReservedBefore { get; set; }


    }
}

namespace Capstone_Backend.Models
{
    public class ReviewDTO
    {
        public string Content { get; set; }
        public double Value { get; set; }
        public DateTime DateRated { get; set; }
        public string ReviewerId { get; set; }
        public int AppartmentId { get; set; }
    }
}

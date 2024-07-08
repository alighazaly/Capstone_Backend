namespace Capstone_Backend.Models
{
    public class RequestGetDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string DateRange { get; set; }
        public string UserId { get; set; }
        public int AppartmentId { get; set; }
        public string Status { get; set; }
        public string UserProfilePicture { get; set; }
        public string AppartmentImage { get; set; }
    }
}

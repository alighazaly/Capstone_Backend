namespace Capstone_Backend.Models
{
    public class FeedbackGetDTO
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public String Content { get; set; }
        public String writerFirstName { get; set; }
        public String writerLastName { get; set; }
        public string ProfilePicture { get; set; }
        public string ImageSrc { get; set; }
    }
}

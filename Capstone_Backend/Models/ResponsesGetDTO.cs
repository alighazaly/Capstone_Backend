namespace Capstone_Backend.Models
{
    public class ResponsesGetDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string DateRange { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
        public int? ReservationId { get; set; }
        public int RequestId { get; set; }
        public string AppartmentImage { get; set; }

    }
}

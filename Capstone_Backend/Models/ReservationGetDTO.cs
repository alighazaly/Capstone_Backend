namespace Capstone_Backend.Models
{
    public class ReservationGetDTO
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public int AppartmentId { get; set; }
        public int? ResponsesId { get; set; }
        public string Date { get; set; }
        public string Content { get; set; }
        public string AppartmentImage { get; set; }

    }
}

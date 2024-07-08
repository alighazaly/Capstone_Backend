using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class Responses
    {
        public int Id { get; set; }

        public String Content { get; set; }
        public string DateRange { get; set; } // Combined StartDate and EndDate

        public String Status { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser user { get; set; }

        public int? ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public Reservation reservation { get; set; }

        public int RequestId { get; set; }
        [ForeignKey("RequestId")]
        public Requests requests { get; set; }


    }
}

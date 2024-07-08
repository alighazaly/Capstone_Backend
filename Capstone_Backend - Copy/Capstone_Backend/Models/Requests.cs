using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class Requests
    {
        public int Id { get; set; }

        public String Content { get;set; }
        public string DateRange { get; set; } // Combined StartDate and EndDate
        public string UserId { get; set; }
        public string ownerId { get; set; }
        [ForeignKey("UserId")]
        public AppUser user { get; set; }

        public int AppartmentId { get; set; }
        [ForeignKey("AppartmentId")]
        public Appartment appartment { get; set; }

        public int ResponsesId { get; set; }
        [ForeignKey("ResponsesId")]
        public Responses responses { get; set; }
        public string Status { get; set; } // Status for the request


    }
}

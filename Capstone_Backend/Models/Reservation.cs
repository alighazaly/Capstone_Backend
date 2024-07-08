using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public string CustomerId { get; set; }

        public AppUser Customer { get; set; }

        [ForeignKey("Appartment")]
        public int AppartmentId { get; set; }

        public Appartment Appartment { get; set; }

        public int ResponsesId { get; set; }
        [ForeignKey("ResponsesId")]
        public Responses? responses { get; set; }
        public String Date {  get; set; }
        public String Content { get; set; }
        public string? AppartmentImage { get; set; }

    }
}

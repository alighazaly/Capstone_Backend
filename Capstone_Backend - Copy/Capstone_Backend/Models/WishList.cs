using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_Backend.Models
{
    public class WishList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // This configures the Id property to be auto-generated
        public int Id { get; set; }
        public ICollection<AppartmentWishList>? appartments { get; set; }

        public String UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser user { get; set; }

    }
}

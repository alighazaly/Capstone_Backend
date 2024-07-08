using System.ComponentModel.DataAnnotations;

namespace Capstone_Backend.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public ICollection<Appartment> Appartments { get; set; }
    }
}

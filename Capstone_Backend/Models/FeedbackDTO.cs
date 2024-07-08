using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Capstone_Backend.Models
{
    public class FeedbackDTO
    {
        public int Value { get; set; }
        public String Content { get; set; }
        public string WriterId { get; set; }
    }
}

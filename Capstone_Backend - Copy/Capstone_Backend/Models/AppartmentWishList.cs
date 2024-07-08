namespace Capstone_Backend.Models
{
    public class AppartmentWishList
    {
        public int AppartmentId { get; set; }
        public Appartment Appartment { get; set; }

        public int WishListId { get; set; }
        public WishList WishList { get; set; }
    }
}

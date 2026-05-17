namespace GiftBoxy.Domain.Entities
{
    public class Wishlist : BaseEntity
    {
        public string Name { get; set; } = "My Wishlist";
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public ICollection<WishlistItem> WishlistItems { get; set; }
    }
}

namespace GiftBoxy.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
    }
}

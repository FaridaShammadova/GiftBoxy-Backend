using GiftBoxy.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace GiftBoxy.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string? Avatar { get; set; }
        public UserRole Role { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpireDate { get; set; }

        public SellerProfile SellerProfile { get; set; }
        public BuyerProfile BuyerProfile { get; set; }
        public ICollection<Product>? Products { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
        public ICollection<Wishlist>? Wishlist { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<ProductQuestion>? AskedQuestions { get; set; }
        public ICollection<ProductQuestion>? ReceivedQuestions { get; set; }
        public ICollection<Message>? SentMessages { get; set; }
        public ICollection<Message>? ReceivedMessages { get; set; }

    }
}

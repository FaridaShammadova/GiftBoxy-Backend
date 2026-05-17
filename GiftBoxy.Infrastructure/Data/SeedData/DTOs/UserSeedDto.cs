using GiftBoxy.Domain.Enums;

namespace GiftBoxy.Infrastructure.Data.SeedData.DTOs
{
    public class UserSeedDto
    {
        public int Id { get; set; }
        public UserRole Role { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Avatar { get; set; }

        public ICollection<int>? Wishlist { get; set; }
        public ICollection<int>? Orders { get; set; }

    }
}

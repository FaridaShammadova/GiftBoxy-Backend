namespace GiftBoxy.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public double Rating { get; set; }
        public int StockCount { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsPersonalized { get; set; }
        public bool IsNew { get; set; }
        public string? Badge { get; set; }
        public string BudgetRange { get; set; }

        public int? SellerProfileId { get; set; }
        public SellerProfile? SellerProfile { get; set; }

        public string? UserId { get; set; }
        public AppUser? User { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<ProductImage> Images { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<Wishlist> Wishlist { get; set; }
        public ICollection<ProductQuestion> Questions { get; set; }
        public ICollection<ProductRecipientTag> RecipientTags { get; set; }
        public ICollection<ProductOccasionTag> OccasionTags { get; set; }
        public ICollection<ProductInterestTag> InterestTags { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}

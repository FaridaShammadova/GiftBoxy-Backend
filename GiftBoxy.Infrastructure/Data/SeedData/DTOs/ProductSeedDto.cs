namespace GiftBoxy.Infrastructure.Data.SeedData.DTOs
{
    public class ProductSeedDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public double Rating { get; set; }
        public int Stock { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsPersonalized { get; set; }
        public bool IsNew { get; set; }
        public string Badge { get; set; }
        public string BudgetRange { get; set; }

        public string? UserId { get; set; }

        public int SellerProfileId { get; set; }

        public List<string> Images { get; set; } = [];
        public List<string> RecipientTags { get; set; } = [];
        public List<string> OccasionTags { get; set; } = [];
        public List<string> InterestTags { get; set; } = [];
    }
}

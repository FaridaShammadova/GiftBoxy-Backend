namespace GiftBoxy.Application.DTOs.Product
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
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
        public string CategoryName { get; set; }
        public string SellerStoreName { get; set; }

        public int? SellerId { get; set; }

        public List<string> Images { get; set; } = new();
        public List<string> RecipientTags { get; set; } = new();
        public List<string> OccasionTags { get; set; } = new();
        public List<string> InterestTags { get; set; } = new();
    }
}

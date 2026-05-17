using Microsoft.AspNetCore.Http;

namespace GiftBoxy.Application.DTOs.Product
{
    public class ProductCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockCount { get; set; }
        public bool IsPersonalized { get; set; }
        public string BudgetRange { get; set; }

        public int CategoryId { get; set; }

        public List<IFormFile>? Images { get; set; } = new();  // ← çox şəkil

        // Gift Finder tagları
        public List<string> RecipientTags { get; set; } = new();
        public List<string> OccasionTags { get; set; } = new();
        public List<string> InterestTags { get; set; } = new();
    }
}

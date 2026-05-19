namespace GiftBoxy.Application.DTOs.SellerProfile
{
    public class SellerProfileResponseDto
    {
        public int Id { get; set; }
        public string StoreName { get; set; }
        public string? ShopUrl { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public double Rating { get; set; }
        public int TotalSales { get; set; }
        public int Followers { get; set; }

        public List<string> Categories { get; set; } = new();
    }
}

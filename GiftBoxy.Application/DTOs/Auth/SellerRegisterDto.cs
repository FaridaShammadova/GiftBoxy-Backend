namespace GiftBoxy.Application.DTOs.Auth
{
    public class SellerRegisterDto
    {
        public string FullName { get; set; }
        public string StoreName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ShopUrl { get; set; }

        public List<string> Categories { get; set; } = new List<string>();
    }
}

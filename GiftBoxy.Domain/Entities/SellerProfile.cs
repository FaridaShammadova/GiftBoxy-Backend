namespace GiftBoxy.Domain.Entities
{
    public class SellerProfile : BaseEntity
    {
        public string StoreName { get; set; }
        public string ShopUrl { get; set; }
        public string Avatar { get; set; }
        public string Bio { get; set; }
        public string Location { get; set; }
        public double Rating { get; set; }
        public int TotalSales { get; set; }
        public int Followers { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public ICollection<Product> Products { get; set; }
        public ICollection<SellerCategory> SellerCategories { get; set; }
    }
}

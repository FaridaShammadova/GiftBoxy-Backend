namespace GiftBoxy.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Icon { get; set; }

        public ICollection<Product> Products { get; set; }
        public ICollection<SellerCategory> SellerCategories { get; set; }
    }
}

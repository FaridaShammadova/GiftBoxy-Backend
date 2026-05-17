namespace GiftBoxy.Domain.Entities
{
    public class SellerCategory
    {
        public int SellerProfileId { get; set; }
        public SellerProfile SellerProfile { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}

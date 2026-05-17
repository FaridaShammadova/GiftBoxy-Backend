namespace GiftBoxy.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public string ImageUrl { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}

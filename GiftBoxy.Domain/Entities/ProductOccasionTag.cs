namespace GiftBoxy.Domain.Entities
{
    public class ProductOccasionTag : BaseEntity
    {
        public string Name { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}

namespace GiftBoxy.Domain.Entities
{
    public class ProductRecipientTag : BaseEntity
    {
        public string Name { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}

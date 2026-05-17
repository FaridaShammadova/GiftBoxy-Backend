namespace GiftBoxy.Infrastructure.Data.SeedData.DTOs
{
    public class CartItemSeedDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        public int CartId { get; set; }

        public int ProductId { get; set; }
    }
}

namespace GiftBoxy.Application.DTOs.Order
{
    public class OrderItemResponseDto
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }
}

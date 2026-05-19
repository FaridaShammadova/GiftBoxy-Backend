namespace GiftBoxy.Application.DTOs.Cart
{
    public class CartResponseDto
    {
        public decimal Total { get; set; }

        public int CartId { get; set; }

        public List<CartItemResponseDto> Items { get; set; } = new();
    }
}

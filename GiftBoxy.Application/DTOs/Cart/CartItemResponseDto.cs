using System;
using System.Collections.Generic;
using System.Text;

namespace GiftBoxy.Application.DTOs.Cart
{
    public class CartItemResponseDto
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public int StockCount { get; set; }
    }
}

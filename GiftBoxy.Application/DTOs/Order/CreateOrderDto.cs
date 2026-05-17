using GiftBoxy.Domain.Enums;

namespace GiftBoxy.Application.DTOs.Order
{
    public class CreateOrderDto
    {
        public string ShippingAddress { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? CouponCode { get; set; }
    }
}

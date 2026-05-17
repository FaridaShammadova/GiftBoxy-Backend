using GiftBoxy.Domain.Enums;

namespace GiftBoxy.Domain.Entities
{
    public class Order : BaseEntity
    {
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string ShippingAddress { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}

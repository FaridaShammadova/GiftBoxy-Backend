using GiftBoxy.Domain.Enums;

namespace GiftBoxy.Application.DTOs.Order
{
    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
    }
}

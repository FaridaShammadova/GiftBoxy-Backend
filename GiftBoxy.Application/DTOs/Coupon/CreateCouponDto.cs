namespace GiftBoxy.Application.DTOs.Coupon
{
    public class CreateCouponDto
    {
        public string Code { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal MinimumAmount { get; set; }
        public int UsageLimit { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}

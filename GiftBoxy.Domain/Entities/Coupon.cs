namespace GiftBoxy.Domain.Entities
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal MinimumAmount { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;

        public string SellerId { get; set; }
        public AppUser Seller { get; set; }
    }
}

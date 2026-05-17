namespace GiftBoxy.Domain.Entities
{
    public class Conversation : BaseEntity
    {
        public string BuyerId { get; set; }
        public AppUser Buyer { get; set; }

        public string SellerId { get; set; }
        public AppUser Seller { get; set; }

        public ICollection<Message> Messages { get; set; }
    }
}

namespace GiftBoxy.Domain.Entities
{
    public class ProductQuestion : BaseEntity
    {
        public string QuestionText { get; set; }
        public string? AnswerText { get; set; }
        public DateTime? AnsweredAt { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public string SellerId { get; set; }
        public AppUser Seller { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}

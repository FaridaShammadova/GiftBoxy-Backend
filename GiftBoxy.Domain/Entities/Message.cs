namespace GiftBoxy.Domain.Entities
{
    public class Message : BaseEntity
    {
        public string Text { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }

        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public string SenderId { get; set; }
        public AppUser Sender { get; set; }
    }
}

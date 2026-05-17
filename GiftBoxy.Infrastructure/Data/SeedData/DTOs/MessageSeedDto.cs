namespace GiftBoxy.Infrastructure.Data.SeedData.DTOs
{
    public class MessageSeedDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsRead { get; set; }
        public string SentAt { get; set; }

        public int ConversationId { get; set; }

        public string SenderId { get; set; }
    }
}

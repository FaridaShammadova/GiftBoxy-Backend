namespace GiftBoxy.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; }
        public string Comment { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}

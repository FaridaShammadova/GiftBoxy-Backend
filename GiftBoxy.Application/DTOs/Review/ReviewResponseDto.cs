namespace GiftBoxy.Application.DTOs.Review
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string UserName { get; set; }
        public string? UserAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

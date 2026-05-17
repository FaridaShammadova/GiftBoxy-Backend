namespace GiftBoxy.Application.DTOs.Review
{
    public class CreateReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
        public int ProductId { get; set; }
    }
}

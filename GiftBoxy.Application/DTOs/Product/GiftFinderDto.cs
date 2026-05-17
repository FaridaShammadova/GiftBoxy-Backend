namespace GiftBoxy.Application.DTOs.Product
{
    public class GiftFinderDto
    {
        public string? Occasion { get; set; }    // "8mart", "sevgililer_gunu"
        public string? Recipient { get; set; }   // "sevgili", "ana", "dost"
        public string? Interest { get; set; }    // "romantik", "gulmeli"
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
    }
}

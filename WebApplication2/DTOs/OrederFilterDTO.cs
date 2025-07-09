namespace WebApplication2.DTOs
{
    public class OrderFilterDTO
    {
        public DateTime? From { get; set; }   
        public DateTime? To { get; set; }   
        public string? Email { get; set; }   
        public decimal? MinTotal { get; set; }
        public decimal? MaxTotal { get; set; }
    }
}

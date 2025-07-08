namespace WebApplication2
{
    public class OrderResponseDTO
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<string> Items { get; set; }
    }
}

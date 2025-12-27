namespace SupportTicketsAPI.DTOs.Dashboard
{
    public class TicketsByProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Count { get; set; }
    }
}



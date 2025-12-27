namespace SupportTicketsAPI.DTOs
{
    public class CreateTicketRequestDto
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public int ProductId { get; set; }
    }
}

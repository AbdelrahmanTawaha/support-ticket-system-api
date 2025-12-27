namespace SupportTicketsAPI.DTOs
{
    public class UpdateTicketDetailsRequestDto
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
    }
}

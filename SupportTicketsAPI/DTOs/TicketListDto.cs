namespace SupportTicketsAPI.DTOs
{
    public class TicketListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string? ClientName { get; set; }
        public string? AssignedEmployeeName { get; set; }
        public string? ProductName { get; set; }
    }
}

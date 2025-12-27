using DataAccessLayer.ConfigurationsSetting.Enums;

namespace SupportTicketsAPI.DTOs
{
    public class TicketDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketStatus Status { get; set; }

        public string ClientName { get; set; } = null!;
        public string? AssignedEmployeeName { get; set; }
        public string ProductName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public List<TicketCommentDto> Comments { get; set; } = new();
    }
}

using DataAccessLayer.ConfigurationsSetting.Enums;

namespace BusinessLayer.Models
{
    public class TicketDetailsModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketStatus Status { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public string? AssignedEmployeeName { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public List<TicketCommentModel> Comments { get; set; } = new();
    }
}

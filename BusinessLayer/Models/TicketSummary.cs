using DataAccessLayer.ConfigurationsSetting.Enums;

namespace BusinessLayer.Models
{
    public class TicketSummary
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public TicketStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? ClientName { get; set; }

        public string? AssignedEmployeeName { get; set; }

        public string? ProductName { get; set; }
    }
}

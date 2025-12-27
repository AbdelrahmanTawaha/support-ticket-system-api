using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.ConfigurationsSetting.AiViews
{
    [Keyless]

    public class TicketAiReportRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int ClientId { get; set; }
        public string? ClientFullName { get; set; }
        public string? ClientUserName { get; set; }
        public string? ClientEmail { get; set; }

        public int? AssignedEmployeeId { get; set; }
        public string? EmployeeFullName { get; set; }
        public string? EmployeeUserName { get; set; }
        public string? EmployeeEmail { get; set; }

        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public bool? ProductIsActive { get; set; }
    }
}

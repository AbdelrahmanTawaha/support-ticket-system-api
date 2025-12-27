using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.ConfigurationsSetting.AiViews
{
    [Keyless]

    public class TicketAiSafeRow
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ClientId { get; set; }
        public int? AssignedEmployeeId { get; set; }
        public int ProductId { get; set; }
    }
}

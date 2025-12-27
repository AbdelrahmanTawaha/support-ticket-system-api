using DataAccessLayer.ConfigurationsSetting.Enums;

namespace SupportTicketsAPI.DTOs
{
    public class UserTicketCountDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public UserType UserType { get; set; }
        public bool IsActive { get; set; }
        public int TicketsCount { get; set; }
        public string? ImageUrl { get; set; }

    }
}

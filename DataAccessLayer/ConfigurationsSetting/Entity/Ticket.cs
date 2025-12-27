using DataAccessLayer.ConfigurationsSetting.Enums;

namespace DataAccessLayer.ConfigurationsSetting.Entity
{
    public class Ticket
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        public int ClientId { get; set; }
        public User Client { get; set; } = null!;


        public int? AssignedEmployeeId { get; set; }
        public User? AssignedEmployee { get; set; }


        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    }
}

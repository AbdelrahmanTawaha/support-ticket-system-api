namespace DataAccessLayer.ConfigurationsSetting.Entity
{
    public class TicketComment
    {
        public int Id { get; set; }

        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int AuthorId { get; set; }
        public User Author { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public bool IsFromSupportTeam { get; set; }
    }
}

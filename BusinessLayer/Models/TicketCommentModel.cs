namespace BusinessLayer.Models
{
    public class TicketCommentModel
    {
        public int Id { get; set; }

        public string CommentText { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;


        public bool IsFromSupportTeam { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

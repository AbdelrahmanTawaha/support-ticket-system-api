namespace SupportTicketsAPI.DTOs
{
    public class TicketCommentDto
    {
        public int Id { get; set; }
        public string CommentText { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public bool IsFromClient { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

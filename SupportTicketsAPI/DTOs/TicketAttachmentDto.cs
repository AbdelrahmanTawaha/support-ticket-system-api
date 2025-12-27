namespace SupportTicketsAPI.DTOs
{
    public class TicketAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public long FileSizeInBytes { get; set; }
        public DateTime UploadedAt { get; set; }

        public int TicketId { get; set; }

        public int? UploadedByUserId { get; set; }
        public string? UploadedByName { get; set; }
    }
}

namespace BusinessLayer.Models
{
    public class TicketAttachmentModel
    {
        public int Id { get; set; }
        public int TicketId { get; set; }

        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long FileSizeInBytes { get; set; }
        public DateTime UploadedAt { get; set; }

        public int? UploadedByUserId { get; set; }
        public string? UploadedByName { get; set; }
    }
}

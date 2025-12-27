namespace DataAccessLayer.ConfigurationsSetting.Entity
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!; // 
        public long FileSizeInBytes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;


        public int? UploadedByUserId { get; set; }
        public User? UploadedByUser { get; set; }

    }
}

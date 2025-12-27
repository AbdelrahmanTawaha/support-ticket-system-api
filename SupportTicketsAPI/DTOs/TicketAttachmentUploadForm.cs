using System.ComponentModel.DataAnnotations;

namespace SupportTicketsAPI.DTOs
{
    public class TicketAttachmentUploadForm
    {
        [Required]
        public IFormFile File { get; set; } = default!;
    }
}

namespace SupportTicketsAPI.Services.Files
{
    public interface ITicketAttachmentStorage
    {
        Task<(string relativePath, long size, string originalFileName)> SaveAsync(
            int ticketId,
            IFormFile file,
            CancellationToken ct = default);

        Task<bool> DeletePhysicalAsync(string relativePath, CancellationToken ct = default);
    }
}

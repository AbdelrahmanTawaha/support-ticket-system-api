using DataAccessLayer.ConfigurationsSetting.Entity;

namespace DataAccessLayer.Repositories.Attachment
{
    public interface ITicketAttachmentRepository : IGenericRepository<TicketAttachment>
    {
        Task<List<TicketAttachment>> GetByTicketIdAsync(int ticketId);
        Task<TicketAttachment?> GetWithUploaderAsync(int id);
    }
}

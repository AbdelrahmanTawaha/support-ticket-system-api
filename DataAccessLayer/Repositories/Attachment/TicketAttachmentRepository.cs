using DataAccessLayer.ConfigurationsSetting;
using DataAccessLayer.ConfigurationsSetting.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer.Repositories.Attachment
{
    public class TicketAttachmentRepository
        : GenericRepository<TicketAttachment>, ITicketAttachmentRepository
    {
        private readonly ILogger<TicketAttachmentRepository> _logger;

        public TicketAttachmentRepository(AppDbContext context, ILogger<TicketAttachmentRepository> logger)
            : base(context)
        {
            _logger = logger;
        }

        public async Task<List<TicketAttachment>> GetByTicketIdAsync(int ticketId)
        {
            try
            {
                return await _context.TicketAttachments
                    .AsNoTracking()
                    .Include(x => x.UploadedByUser)
                    .Where(x => x.TicketId == ticketId)
                    .OrderByDescending(x => x.UploadedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachments by ticketId. ticketId={TicketId}", ticketId);
                throw;
            }
        }

        public async Task<TicketAttachment?> GetWithUploaderAsync(int id)
        {
            try
            {
                return await _context.TicketAttachments
                    .Include(x => x.UploadedByUser)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachment with uploader. id={Id}", id);
                throw;
            }
        }
    }
}

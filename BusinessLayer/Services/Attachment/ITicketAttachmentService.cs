
using BusinessLayer.Models;
using BusinessLayer.Responses;
namespace BusinessLayer.Services.Attachment
{
    public interface ITicketAttachmentService
    {



        Task<Response<List<TicketAttachmentModel>>> GetByTicketIdAsync(int ticketId);

        Task<Response<TicketAttachmentModel>> UploadAsync(
            int ticketId,
            int uploaderUserId,
            string fileName,
            string relativePath,
            long sizeInBytes);



        Task<Response<string>> DeleteAsync(int ticketId, int attachmentId, int requesterUserId);


    }
}

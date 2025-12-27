
using BusinessLayer.Models;
using BusinessLayer.Responses;
using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.Repositories.Attachment;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Services.Attachment
{
    public class TicketAttachmentService : ITicketAttachmentService
    {
        private readonly ITicketAttachmentRepository _repo;
        private readonly ILogger<TicketAttachmentService> _logger;

        public TicketAttachmentService(
            ITicketAttachmentRepository repo,
            ILogger<TicketAttachmentService> logger)
        {
            _repo = repo;
            _logger = logger;
        }





        public async Task<Response<List<TicketAttachmentModel>>> GetByTicketIdAsync(int ticketId)
        {
            var response = new Response<List<TicketAttachmentModel>>();

            try
            {
                if (ticketId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid ticket id.";
                    return response;
                }

                var list = await _repo.GetByTicketIdAsync(ticketId);

                response.Data = list.Select(x => new TicketAttachmentModel
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    FilePath = x.FilePath,
                    FileSizeInBytes = x.FileSizeInBytes,
                    UploadedAt = x.UploadedAt,
                    TicketId = x.TicketId,
                    UploadedByUserId = x.UploadedByUserId,
                    UploadedByName = x.UploadedByUser != null ? x.UploadedByUser.FullName : null
                }).ToList();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByTicketIdAsync failed. TicketId={TicketId}", ticketId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = null;
            }

            return response;
        }

        public async Task<Response<TicketAttachmentModel>> UploadAsync(
            int ticketId,
            int uploaderUserId,
            string fileName,
            string relativePath,
            long sizeInBytes)
        {
            var response = new Response<TicketAttachmentModel>();

            try
            {
                if (ticketId <= 0 || uploaderUserId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid ticket id or uploader.";
                    return response;
                }

                var entity = new TicketAttachment
                {
                    TicketId = ticketId,
                    FileName = fileName,
                    FilePath = relativePath,
                    FileSizeInBytes = sizeInBytes,
                    UploadedAt = DateTime.UtcNow,
                    UploadedByUserId = uploaderUserId
                };

                await _repo.AddAsync(entity);
                await _repo.SaveChangesAsync();

                response.Data = new TicketAttachmentModel
                {
                    Id = entity.Id,
                    FileName = entity.FileName,
                    FilePath = entity.FilePath,
                    FileSizeInBytes = entity.FileSizeInBytes,
                    UploadedAt = entity.UploadedAt,
                    TicketId = entity.TicketId,
                    UploadedByUserId = entity.UploadedByUserId
                };

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "UploadAsync failed. TicketId={TicketId}, UploaderId={UploaderId}",
                    ticketId, uploaderUserId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = null;
            }

            return response;
        }

        public async Task<Response<string>> DeleteAsync(int ticketId, int attachmentId, int requesterUserId)
        {
            var response = new Response<string>();

            try
            {
                if (ticketId <= 0 || attachmentId <= 0 || requesterUserId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid data.";
                    response.Data = null;
                    return response;
                }

                var attachment = await _repo.GetWithUploaderAsync(attachmentId);

                if (attachment == null || attachment.TicketId != ticketId)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Attachment not found.";
                    response.Data = null;
                    return response;
                }

                if (attachment.UploadedByUserId != requesterUserId)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "You are not allowed to delete this attachment.";
                    response.Data = null;
                    return response;
                }


                var filePath = attachment.FilePath;

                _repo.Remove(attachment);
                await _repo.SaveChangesAsync();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
                response.Data = filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "DeleteAsync failed. TicketId={TicketId}, AttachmentId={AttachmentId}, RequesterId={RequesterId}",
                    ticketId, attachmentId, requesterUserId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = null;
            }

            return response;
        }

    }
}

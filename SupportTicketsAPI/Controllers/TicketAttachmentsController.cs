using BusinessLayer.Responses;
using BusinessLayer.Services.Attachment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SupportTicketsAPI.DTOs;
using SupportTicketsAPI.Hubs;
using SupportTicketsAPI.Services;
using SupportTicketsAPI.Services.Files;

namespace SupportTicketsAPI.Controllers
{
    [ApiController]
    [Route("api/Tickets")]
    public class TicketAttachmentsController : ControllerBase
    {
        private readonly ITicketAttachmentService _service;
        private readonly ITicketAttachmentStorage _storage;
        private readonly IHubContext<TicketsHub> _hub;
        public TicketAttachmentsController(
            ITicketAttachmentService service,
            ITicketAttachmentStorage storage,
            IHubContext<TicketsHub> hup)
        {
            _service = service;
            _storage = storage;
            _hub = hup;
        }

        [HttpPost("{ticketId:int}/attachments")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Response<TicketAttachmentDto>>> Upload(
    int ticketId,
    IFormFile file,
    CancellationToken ct)
        {
            var api = new Response<TicketAttachmentDto>();

            var uploaderId = ClaimsHelper.GetUserId(User);
            if (uploaderId == null)
            {
                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = "Unauthorized.";
                return Unauthorized(api);
            }

            if (file == null || file.Length == 0)
            {
                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = "File is required.";
                return BadRequest(api);
            }

            var (relativePath, size, originalFileName) =
                await _storage.SaveAsync(ticketId, file, ct);

            var result = await _service.UploadAsync(
                ticketId,
                uploaderId.Value,
                originalFileName,
                relativePath,
                size
            );

            if (result.ErrorCode != ErrorCode.Success || result.Data == null)
            {
                api.ErrorCode = result.ErrorCode;
                api.MsgError = result.MsgError;
                return BadRequest(api);
            }

            api.ErrorCode = ErrorCode.Success;
            api.MsgError = ErrorCode.Success.ToString();
            api.Data = new TicketAttachmentDto
            {
                Id = result.Data.Id,
                TicketId = result.Data.TicketId,
                FileName = result.Data.FileName,
                FilePath = result.Data.FilePath,
                FileSizeInBytes = result.Data.FileSizeInBytes,
                UploadedAt = result.Data.UploadedAt,
                UploadedByUserId = result.Data.UploadedByUserId,
                UploadedByName = result.Data.UploadedByName
            };

            await _hub.Clients
                .Group(TicketsHub.TicketGroup(ticketId))
                .SendAsync("AttachmentAdded", api.Data, ct);

            return Ok(api);
        }


        [HttpGet("{ticketId:int}/attachments")]
        [Authorize]
        public async Task<ActionResult<Response<List<TicketAttachmentDto>>>> GetByTicket(int ticketId)
        {
            var result = await _service.GetByTicketIdAsync(ticketId);

            var api = new Response<List<TicketAttachmentDto>>
            {
                ErrorCode = result.ErrorCode,
                MsgError = result.MsgError,
                Data = result.Data?.Select(x => new TicketAttachmentDto
                {
                    Id = x.Id,
                    TicketId = x.TicketId,
                    FileName = x.FileName,
                    FilePath = x.FilePath,
                    FileSizeInBytes = x.FileSizeInBytes,
                    UploadedAt = x.UploadedAt,
                    UploadedByUserId = x.UploadedByUserId,
                    UploadedByName = x.UploadedByName
                }).ToList()
            };

            if (result.ErrorCode != ErrorCode.Success)
                return BadRequest(api);

            return Ok(api);
        }

        [HttpDelete("{ticketId:int}/attachments/{attachmentId:int}")]
        [Authorize]
        public async Task<ActionResult<Response<bool>>> Delete(
       int ticketId,
       int attachmentId,
       CancellationToken ct)
        {
            var api = new Response<bool>();

            var requesterId = ClaimsHelper.GetUserId(User);
            if (requesterId == null)
            {
                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = "Unauthorized.";
                return Unauthorized(api);
            }


            var result = await _service.DeleteAsync(ticketId, attachmentId, requesterId.Value);

            if (result.ErrorCode != ErrorCode.Success || string.IsNullOrWhiteSpace(result.Data))
            {
                api.ErrorCode = result.ErrorCode;
                api.MsgError = result.MsgError;
                api.Data = false;
                return BadRequest(api);
            }


            var physicalDeleted = await _storage.DeletePhysicalAsync(result.Data, ct);

            api.ErrorCode = ErrorCode.Success;
            api.MsgError = physicalDeleted
                ? ErrorCode.Success.ToString()
                : "Deleted from database, but physical file was not found (or could not be deleted).";

            api.Data = true;

            await _hub.Clients
                .Group(TicketsHub.TicketGroup(ticketId))
                .SendAsync("AttachmentDeleted", attachmentId, ct);

            return Ok(api);
        }

    }
}

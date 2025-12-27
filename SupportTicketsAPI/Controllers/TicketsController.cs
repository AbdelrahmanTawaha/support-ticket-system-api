using System.Security.Claims;
using BusinessLayer.Responses;
using BusinessLayer.Services.AiAssignSuggest;
using BusinessLayer.Services.Ticket.interfaces;
using DataAccessLayer.ConfigurationsSetting.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SupportTicketsAPI.DTOs;
using SupportTicketsAPI.Hubs;

using SupportTicketsAPI.Services;

namespace SupportTicketsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IHubContext<TicketsHub> _hub;
        private readonly ILogger<TicketsController> _logger;
        private readonly IAiAssignSuggestService _aiAssignSuggest;

        public TicketsController(
            ITicketService ticketService,
            IHubContext<TicketsHub> hub,
            ILogger<TicketsController> logger,
            IAiAssignSuggestService aiAssignSuggest)
        {
            _ticketService = ticketService;
            _hub = hub;
            _logger = logger;
            _aiAssignSuggest = aiAssignSuggest;
        }


        private static string TicketGroup(int ticketId) => $"ticket-{ticketId}";


        [Authorize(Roles = AppRoles.Manager)]
        [HttpGet("admin")]
        public async Task<ActionResult<ApiPageResponse<List<TicketListDto>>>> GetAdminTickets(
            int pageNumber = 1,
            int pageSize = 10,
            int? clientId = null,
            int? assignedEmployeeId = null,
            int? productId = null,
            string? status = null,
            string? searchTerm = null)
        {
            var api = new ApiPageResponse<List<TicketListDto>>();

            try
            {
                var result = await _ticketService.GetPagedAsync(
                    pageNumber, pageSize,
                    clientId, assignedEmployeeId, productId,
                    status, searchTerm);

                if (result.ErrorCode != ErrorCode.Success || result.Data == null)
                {
                    api.ErrorCode = (int)result.ErrorCode;
                    api.MsgError = result.MsgError ?? "Failed to load tickets.";
                    api.Data = null;
                    api.TotalCount = 0;
                    return BadRequest(api);
                }

                api.Data = result.Data.Select(t => new TicketListDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status.ToString(),
                    CreatedAt = t.CreatedAt,
                    ClientName = t.ClientName,
                    AssignedEmployeeName = t.AssignedEmployeeName,
                    ProductName = t.ProductName
                }).ToList();

                api.TotalCount = result.TotalCount;
                api.ErrorCode = (int)ErrorCode.Success;
                api.MsgError = ErrorCode.Success.ToString();

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAdminTickets error");

                api.ErrorCode = (int)ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;
                api.TotalCount = 0;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }




        [Authorize(Roles = AppRoles.Employee)]
        [HttpGet("employee")]
        public async Task<ActionResult<ApiPageResponse<List<TicketListDto>>>> GetEmployeeTickets(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? searchTerm = null)
        {
            var api = new ApiPageResponse<List<TicketListDto>>();

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var result = await _ticketService.GetPagedAsync(
                    pageNumber, pageSize,
                    clientId: null,
                    assignedEmployeeId: userId.Value,
                    productId: null,
                    status,
                    searchTerm);

                if (result.ErrorCode != ErrorCode.Success || result.Data == null)
                {
                    api.ErrorCode = (int)result.ErrorCode;
                    api.MsgError = result.MsgError ?? "Failed to load tickets.";
                    api.Data = null;
                    api.TotalCount = 0;
                    return BadRequest(api);
                }

                api.Data = result.Data.Select(t => new TicketListDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status.ToString(),
                    CreatedAt = t.CreatedAt,
                    ClientName = t.ClientName,
                    AssignedEmployeeName = t.AssignedEmployeeName,
                    ProductName = t.ProductName
                }).ToList();

                api.TotalCount = result.TotalCount;
                api.ErrorCode = (int)ErrorCode.Success;
                api.MsgError = ErrorCode.Success.ToString();

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetEmployeeTickets error");

                api.ErrorCode = (int)ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;
                api.TotalCount = 0;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }


        [Authorize(Roles = AppRoles.Client)]
        [HttpGet("client")]
        public async Task<ActionResult<ApiPageResponse<List<TicketListDto>>>> GetClientTickets(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? searchTerm = null)
        {
            var api = new ApiPageResponse<List<TicketListDto>>();

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var result = await _ticketService.GetPagedAsync(
                    pageNumber, pageSize,
                    clientId: userId.Value,
                    assignedEmployeeId: null,
                    productId: null,
                    status,
                    searchTerm);

                if (result.ErrorCode != ErrorCode.Success || result.Data == null)
                {
                    api.ErrorCode = (int)result.ErrorCode;
                    api.MsgError = result.MsgError ?? "Failed to load tickets.";
                    api.Data = null;
                    api.TotalCount = 0;
                    return BadRequest(api);
                }

                api.Data = result.Data.Select(t => new TicketListDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status.ToString(),
                    CreatedAt = t.CreatedAt,
                    ClientName = t.ClientName,
                    AssignedEmployeeName = t.AssignedEmployeeName,
                    ProductName = t.ProductName
                }).ToList();

                api.TotalCount = result.TotalCount;
                api.ErrorCode = (int)ErrorCode.Success;
                api.MsgError = ErrorCode.Success.ToString();

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClientTickets error");

                api.ErrorCode = (int)ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;
                api.TotalCount = 0;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }


        [Authorize(Roles = AppRoles.Manager + "," + AppRoles.Employee + "," + AppRoles.Client)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Response<TicketDetailsDto>>> GetById(int id)
        {
            var api = new Response<TicketDetailsDto>();

            try
            {
                if (id <= 0)
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Invalid ticket id.";
                    return BadRequest(api);
                }

                var result = await _ticketService.GetByIdAsync(id);

                if (result.ErrorCode != ErrorCode.Success || result.Data == null)
                {
                    api.ErrorCode = result.ErrorCode;
                    api.MsgError = result.MsgError ?? "Ticket not found.";
                    return NotFound(api);
                }

                var m = result.Data;

                api.Data = new TicketDetailsDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Status = m.Status,
                    CreatedAt = m.CreatedAt,
                    ClientName = m.ClientName,
                    AssignedEmployeeName = m.AssignedEmployeeName,
                    ProductName = m.ProductName,
                    Comments = m.Comments.Select(c => new TicketCommentDto
                    {
                        Id = c.Id,
                        CommentText = c.CommentText,
                        CreatedAt = c.CreatedAt,
                        AuthorName = c.AuthorName,
                        IsFromClient = !c.IsFromSupportTeam
                    }).ToList()
                };

                api.ErrorCode = ErrorCode.Success;
                api.MsgError = ErrorCode.Success.ToString();

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById error");

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }


        [Authorize(Roles = AppRoles.Manager + "," + AppRoles.Employee + "," + AppRoles.Client)]
        [HttpPost("{id:int}/comments")]
        public async Task<ActionResult<Response<TicketCommentDto>>> AddComment(
            int id,
            [FromBody] AddCommentRequestDto dto)
        {
            var api = new Response<TicketCommentDto>();

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var result = await _ticketService.AddCommentAsync(id, userId.Value, dto.CommentText);

                if (result.ErrorCode != ErrorCode.Success || result.Data == null)
                {
                    api.ErrorCode = result.ErrorCode;
                    api.MsgError = result.MsgError ?? "Failed to add comment.";
                    api.Data = null;
                    return BadRequest(api);
                }

                api.Data = new TicketCommentDto
                {
                    Id = result.Data.Id,
                    CommentText = result.Data.CommentText,
                    CreatedAt = result.Data.CreatedAt,
                    AuthorName = result.Data.AuthorName,
                    IsFromClient = !result.Data.IsFromSupportTeam
                };

                api.ErrorCode = ErrorCode.Success;
                api.MsgError = ErrorCode.Success.ToString();

                await _hub.Clients
                    .Group(TicketGroup(id))
                    .SendAsync("CommentAdded", api.Data);

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddComment error");

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }


        [Authorize(Roles = AppRoles.Manager)]
        [HttpPut("{id:int}/assign")]
        public async Task<ActionResult<Response<bool>>> AssignTicket(int id, [FromBody] AssignTicketDto dto)
        {
            var api = new Response<bool>();

            try
            {
                if (id <= 0 || dto.EmployeeId <= 0)
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Invalid data.";
                    api.Data = false;
                    return BadRequest(api);
                }

                var result = await _ticketService.AssignTicketAsync(id, dto.EmployeeId);

                api.ErrorCode = result.ErrorCode;
                api.MsgError = result.MsgError;
                api.Data = result.Data;

                if (result.ErrorCode != ErrorCode.Success)
                    return BadRequest(api);

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AssignTicket error");

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = false;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

        [Authorize(Roles = AppRoles.Employee + "," + AppRoles.Manager)]
        [HttpPut("{id:int}/waiting-client")]
        public async Task<ActionResult<Response<bool>>> MarkWaitingClient(int id)
        {
            var api = new Response<bool>();

            try
            {
                var userId = ClaimsHelper.GetUserId(User);
                if (userId == null) return Unauthorized();

                var result = await _ticketService.MarkWaitingClientAsync(id, userId.Value);

                api.ErrorCode = result.ErrorCode;
                api.MsgError = result.MsgError;
                api.Data = result.Data;

                if (result.ErrorCode != ErrorCode.Success)
                    return BadRequest(api);

                await _hub.Clients
                    .Group(TicketGroup(id))
                    .SendAsync("StatusChanged", new { ticketId = id, status = TicketStatus.WaitingClient });

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarkWaitingClient error");

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = false;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

        [Authorize(Roles = AppRoles.Client)]
        [HttpPut("{id:int}/client-decision")]
        public async Task<ActionResult<Response<bool>>> ClientDecision(int id, [FromBody] ClientDecisionDto dto)
        {
            var api = new Response<bool>();

            try
            {
                var userId = ClaimsHelper.GetUserId(User);
                if (userId == null) return Unauthorized();

                if (dto == null || string.IsNullOrWhiteSpace(dto.Action))
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Action is required.";
                    api.Data = false;
                    return BadRequest(api);
                }

                Response<bool> result;
                TicketStatus newStatus;

                switch (dto.Action.Trim().ToLowerInvariant())
                {
                    case "confirm":
                        result = await _ticketService.ConfirmFixAsync(id, userId.Value);
                        newStatus = TicketStatus.Closed;
                        break;

                    case "reject":
                        result = await _ticketService.RejectFixAsync(id, userId.Value);
                        newStatus = TicketStatus.InProgress;
                        break;

                    default:
                        api.ErrorCode = ErrorCode.GeneralError;
                        api.MsgError = "Invalid action. Use confirm or reject.";
                        api.Data = false;
                        return BadRequest(api);
                }

                api.ErrorCode = result.ErrorCode;
                api.MsgError = result.MsgError;
                api.Data = result.Data;

                if (result.ErrorCode != ErrorCode.Success)
                    return BadRequest(api);

                await _hub.Clients.Group(TicketGroup(id))
                    .SendAsync("StatusChanged", new { ticketId = id, status = newStatus });

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClientDecision error");
                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = false;
                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }
        [Authorize(Roles = AppRoles.Client)]
        [HttpPost("client")]
        public async Task<ActionResult<Response<int>>> CreateClientTicket([FromBody] CreateTicketRequestDto dto)
        {
            var api = new Response<int>();

            try
            {
                var userId = ClaimsHelper.GetUserId(User);
                if (userId == null) return Unauthorized(api);

                var result = await _ticketService.CreateClientTicketAsync(
                    userId.Value,
                    dto.Title,
                    dto.Description,
                    dto.ProductId);

                api.ErrorCode = result.ErrorCode;
                api.MsgError = result.MsgError;
                api.Data = result.Data;

                if (result.ErrorCode != ErrorCode.Success)
                    return BadRequest(api);

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateClientTicket error");

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = 0;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }
        [Authorize(Roles = AppRoles.Manager)]
        [HttpPost("{id:int}/ai-assign-suggest")]
        public async Task<ActionResult<ApiResponse<AiAssignSuggestResponseDto>>> AiAssignSuggest(int id, CancellationToken ct)
        {
            var api = new ApiResponse<AiAssignSuggestResponseDto>();

            try
            {
                var r = await _aiAssignSuggest.SuggestAsync(id, ct);

                api.Data = new AiAssignSuggestResponseDto
                {
                    SuggestedEmployeeId = r.SuggestedEmployeeId,
                    SuggestedEmployeeName = r.SuggestedEmployeeName,
                    Confidence = r.Confidence,
                    Reason = r.Reason,
                    IsFallback = r.IsFallback,
                    Warning = r.Warning
                };

                api.ErrorCode = (int)ErrorCode.Success;
                api.MsgError = ErrorCode.Success.ToString();
                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AiAssignSuggest failed. ticketId={TicketId}", id);

                api.ErrorCode = (int)ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;

                return StatusCode(500, api);
            }
        }

        [HttpPut("{id:int}/details")]
        public async Task<ActionResult<Response<TicketDetailsDto>>> UpdateTicketDetails(
    int id,
    [FromBody] UpdateTicketDetailsRequestDto dto)
        {
            var api = new Response<TicketDetailsDto>();

            try
            {
                var userId = ClaimsHelper.GetUserId(User);
                if (userId == null) return Unauthorized(api);

                var result = await _ticketService.UpdateDetailsAsync(
                    id,
                    userId.Value,
                    dto?.Title ?? "",
                    dto?.Description
                );

                if (result.ErrorCode != ErrorCode.Success || result.Data == null)
                {
                    api.ErrorCode = result.ErrorCode;
                    api.MsgError = result.MsgError ?? "Failed to update ticket details.";
                    api.Data = null;
                    return BadRequest(api);
                }

                var m = result.Data;

                api.Data = new TicketDetailsDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Status = m.Status,
                    CreatedAt = m.CreatedAt,
                    ClientName = m.ClientName,
                    AssignedEmployeeName = m.AssignedEmployeeName,
                    ProductName = m.ProductName,
                    Comments = m.Comments.Select(c => new TicketCommentDto
                    {
                        Id = c.Id,
                        CommentText = c.CommentText,
                        CreatedAt = c.CreatedAt,
                        AuthorName = c.AuthorName,
                        IsFromClient = !c.IsFromSupportTeam
                    }).ToList()
                };

                api.ErrorCode = ErrorCode.Success;
                api.MsgError = ErrorCode.Success.ToString();


                await _hub.Clients
                    .Group(TicketGroup(id))
                    .SendAsync("TicketUpdated", api.Data);

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateTicketDetails error. ticketId={TicketId}", id);

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out var id) ? id : null;
        }
    }
}

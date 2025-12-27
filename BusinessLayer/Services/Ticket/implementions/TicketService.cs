
using BusinessLayer.Models;
using BusinessLayer.Responses;
using BusinessLayer.Services.Ticket.interfaces;
using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.ConfigurationsSetting.Enums;
using DataAccessLayer.Repositories.ticket.Interface;
using DataAccessLayer.Repositories.user;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Services.Ticket.implementions
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TicketService> _logger;

        public TicketService(
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            ILogger<TicketService> logger)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<PageResponse<List<TicketSummary>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? clientId,
            int? assignedEmployeeId,
            int? productId,
            string? status,
            string? searchTerm)
        {
            var response = new PageResponse<List<TicketSummary>>();

            try
            {
                TicketStatus? statusEnum = null;

                if (!string.IsNullOrWhiteSpace(status) &&
                    Enum.TryParse<TicketStatus>(status, ignoreCase: true, out var parsedStatus))
                {
                    statusEnum = parsedStatus;
                }

                var (items, totalCount) = await _ticketRepository.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    clientId,
                    assignedEmployeeId,
                    productId,
                    statusEnum,
                    searchTerm);

                var list = items.Select(t => new TicketSummary
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,

                    ClientName = t.Client?.FullName ?? t.Client?.UserName ?? string.Empty,
                    AssignedEmployeeName = t.AssignedEmployee?.FullName ?? t.AssignedEmployee?.UserName,
                    ProductName = t.Product?.Name ?? string.Empty
                }).ToList();

                response.Data = list;
                response.TotalCount = totalCount;
                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetPagedAsync failed. pageNumber={PageNumber}, pageSize={PageSize}, clientId={ClientId}, assignedEmployeeId={AssignedEmployeeId}, productId={ProductId}, status={Status}",
                    pageNumber, pageSize, clientId, assignedEmployeeId, productId, status);

                response.Data = null;
                response.TotalCount = 0;
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
            }

            return response;
        }

        public async Task<Response<TicketDetailsModel?>> GetByIdAsync(int id)
        {
            var response = new Response<TicketDetailsModel?>();

            try
            {
                var ticket = await _ticketRepository.GetDetailsByIdAsync(id);

                if (ticket == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket not found.";
                    response.Data = null;
                    return response;
                }



                var model = new TicketDetailsModel
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Status = ticket.Status,
                    CreatedAt = ticket.CreatedAt,

                    ClientName = ticket.Client != null
                        ? (ticket.Client.FullName ?? ticket.Client.UserName)
                        : string.Empty,

                    AssignedEmployeeName = ticket.AssignedEmployee != null
                        ? (ticket.AssignedEmployee.FullName ?? ticket.AssignedEmployee.UserName)
                        : null,

                    ProductName = ticket.Product != null
                        ? ticket.Product.Name
                        : string.Empty,

                    Comments = ticket.Comments?
                        .OrderBy(c => c.CreatedAt)
                        .Select(c => new TicketCommentModel
                        {
                            Id = c.Id,
                            CommentText = c.CommentText,
                            CreatedAt = c.CreatedAt,
                            AuthorName = c.Author != null
                                ? (c.Author.FullName ?? c.Author.UserName)
                                : string.Empty,
                            IsFromSupportTeam = c.IsFromSupportTeam
                        })
                        .ToList() ?? new List<TicketCommentModel>()
                };

                response.Data = model;
                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync failed. id={TicketId}", id);

                response.Data = null;
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
            }

            return response;
        }

        public async Task<Response<TicketCommentModel?>> AddCommentAsync(
            int ticketId,
            int currentUserId,
            string commentText)
        {
            var response = new Response<TicketCommentModel?>();

            try
            {
                if (string.IsNullOrWhiteSpace(commentText))
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Comment text is required.";
                    response.Data = null;
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user is null || !user.IsActive)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "User is inactive or not found.";
                    response.Data = null;
                    return response;
                }

                var ticket = await _ticketRepository.GetForCommentByIdAsync(ticketId);
                if (ticket is null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket not found.";
                    response.Data = null;
                    return response;
                }

                if (ticket.Status == TicketStatus.Closed)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "the ticket  closed. You cant  comments";
                    response.Data = null;
                    return response;



                }
                bool canComment = user.UserType switch
                {
                    UserType.SupportManager => true,
                    UserType.SupportEmployee => ticket.AssignedEmployeeId == user.Id,
                    UserType.ExternalClient => ticket.ClientId == user.Id,
                    _ => false
                };

                if (!canComment)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "You are not allowed to comment on this ticket.";
                    response.Data = null;
                    return response;
                }


                var comment = new TicketComment
                {
                    TicketId = ticket.Id,
                    AuthorId = user.Id,
                    CommentText = commentText.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsFromSupportTeam = user.UserType != UserType.ExternalClient
                };

                await _ticketRepository.AddCommentAsync(comment);

                var model = new TicketCommentModel
                {
                    Id = comment.Id,
                    CommentText = comment.CommentText,
                    CreatedAt = comment.CreatedAt,
                    AuthorName = user.FullName ?? user.UserName,
                    IsFromSupportTeam = comment.IsFromSupportTeam
                };

                response.Data = model;
                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "AddCommentAsync failed. ticketId={TicketId}, currentUserId={UserId}",
                    ticketId, currentUserId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = null;
            }

            return response;
        }

        public async Task<Response<bool>> AssignTicketAsync(int ticketId, int employeeId)
        {
            var response = new Response<bool>();

            try
            {
                if (ticketId <= 0 || employeeId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid assign data.";
                    response.Data = false;
                    return response;
                }

                var employee = await _userRepository.GetByIdAsync(employeeId);
                if (employee == null || !employee.IsActive || employee.UserType != UserType.SupportEmployee)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Employee not found or inactive or not support employee.";
                    response.Data = false;
                    return response;
                }

                var ticket = await _ticketRepository.GetForAssignByIdAsync(ticketId);
                if (ticket == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket not found.";
                    response.Data = false;
                    return response;
                }

                if (ticket.AssignedEmployeeId != null && ticket.AssignedEmployeeId != employeeId)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket already assigned to another employee.";
                    response.Data = false;
                    return response;
                }



                ticket.AssignedEmployeeId = employeeId;

                if (ticket.Status == TicketStatus.New || ticket.Status == TicketStatus.WaitingClient)
                {
                    ticket.Status = TicketStatus.InProgress;
                }

                ticket.UpdatedAt = DateTime.UtcNow;

                _ticketRepository.Update(ticket);
                await _ticketRepository.SaveChangesAsync();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "AssignTicketAsync failed. ticketId={TicketId}, employeeId={EmployeeId}",
                    ticketId, employeeId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = false;
            }

            return response;
        }

        public async Task<Response<List<UserTicketCountModel>>> GetUsersWithTicketsCountAsync()
        {
            var response = new Response<List<UserTicketCountModel>>();

            try
            {
                var data = await _ticketRepository.GetUsersWithTicketsCountAsync();

                response.Data = data.Select(x => new UserTicketCountModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    UserType = x.UserType,
                    IsActive = x.IsActive,
                    TicketsCount = x.TicketsCount
                }).ToList();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUsersWithTicketsCountAsync failed.");

                response.Data = null;
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
            }

            return response;
        }

        public async Task<Response<bool>> MarkWaitingClientAsync(int ticketId, int currentUserId)
        {
            var response = new Response<bool>();

            try
            {
                if (ticketId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid ticket id.";
                    response.Data = false;
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null || !user.IsActive)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "User not found or inactive.";
                    response.Data = false;
                    return response;
                }

                if (user.UserType != UserType.SupportEmployee &&
                    user.UserType != UserType.SupportManager)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Not allowed.";
                    response.Data = false;
                    return response;
                }

                var ticket = await _ticketRepository.GetForStatusChangeByIdAsync(ticketId);
                if (ticket == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket not found.";
                    response.Data = false;
                    return response;
                }

                if (user.UserType == UserType.SupportEmployee &&
                    ticket.AssignedEmployeeId != user.Id)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "You are not assigned to this ticket.";
                    response.Data = false;
                    return response;
                }

                if (ticket.Status != TicketStatus.InProgress)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket must be InProgress before waiting client confirmation.";
                    response.Data = false;
                    return response;
                }

                ticket.Status = TicketStatus.WaitingClient;
                ticket.UpdatedAt = DateTime.UtcNow;

                _ticketRepository.Update(ticket);
                await _ticketRepository.SaveChangesAsync();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "MarkWaitingClientAsync failed. ticketId={TicketId}, currentUserId={UserId}",
                    ticketId, currentUserId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = false;
            }

            return response;
        }

        public async Task<Response<bool>> ConfirmFixAsync(int ticketId, int currentUserId)
        {
            var response = new Response<bool>();

            try
            {
                if (ticketId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid ticket id.";
                    response.Data = false;
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null || !user.IsActive)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "User not found or inactive.";
                    response.Data = false;
                    return response;
                }

                if (user.UserType != UserType.ExternalClient)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Only client can confirm fix.";
                    response.Data = false;
                    return response;
                }

                var ticket = await _ticketRepository.GetForStatusChangeByIdAsync(ticketId);
                if (ticket == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket not found.";
                    response.Data = false;
                    return response;
                }

                if (ticket.ClientId != user.Id)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "This ticket does not belong to you.";
                    response.Data = false;
                    return response;
                }

                if (ticket.Status != TicketStatus.WaitingClient)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket is not waiting for client confirmation.";
                    response.Data = false;
                    return response;
                }

                ticket.Status = TicketStatus.Closed;
                ticket.UpdatedAt = DateTime.UtcNow;

                _ticketRepository.Update(ticket);
                await _ticketRepository.SaveChangesAsync();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ConfirmFixAsync failed. ticketId={TicketId}, currentUserId={UserId}",
                    ticketId, currentUserId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = false;
            }

            return response;
        }

        public async Task<Response<bool>> RejectFixAsync(int ticketId, int currentUserId)
        {
            var response = new Response<bool>();

            try
            {
                if (ticketId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid ticket id.";
                    response.Data = false;
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null || !user.IsActive)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "User not found or inactive.";
                    response.Data = false;
                    return response;
                }

                if (user.UserType != UserType.ExternalClient)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Only client can reject fix.";
                    response.Data = false;
                    return response;
                }

                var ticket = await _ticketRepository.GetForStatusChangeByIdAsync(ticketId);
                if (ticket == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket not found.";
                    response.Data = false;
                    return response;
                }

                if (ticket.ClientId != user.Id)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "This ticket does not belong to you.";
                    response.Data = false;
                    return response;
                }

                if (ticket.Status != TicketStatus.WaitingClient)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket is not waiting for client confirmation.";
                    response.Data = false;
                    return response;
                }

                ticket.Status = TicketStatus.InProgress;
                ticket.UpdatedAt = DateTime.UtcNow;

                _ticketRepository.Update(ticket);
                await _ticketRepository.SaveChangesAsync();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "RejectFixAsync failed. ticketId={TicketId}, currentUserId={UserId}",
                    ticketId, currentUserId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = false;
            }

            return response;
        }

        public async Task<Response<int>> CreateClientTicketAsync(
            int clientId, string title, string? description, int productId)
        {
            var response = new Response<int>();

            try
            {
                if (clientId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid client.";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Title is required.";
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(clientId);
                if (user == null || !user.IsActive || user.UserType != UserType.ExternalClient)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Client not found or inactive.";
                    return response;
                }

                var ticket = new DataAccessLayer.ConfigurationsSetting.Entity.Ticket
                {
                    Title = title.Trim(),
                    Description = description?.Trim(),
                    ClientId = clientId,
                    ProductId = productId,
                    Status = TicketStatus.New,
                    CreatedAt = DateTime.UtcNow
                };

                await _ticketRepository.AddAsync(ticket);
                await _ticketRepository.SaveChangesAsync();

                response.Data = ticket.Id;
                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "CreateClientTicketAsync failed. clientId={ClientId}, productId={ProductId}",
                    clientId, productId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
            }

            return response;
        }
        public async Task<Response<TicketDetailsModel?>> UpdateDetailsAsync(
    int ticketId,
    int currentUserId,
    string title,
    string? description)
        {
            var response = new Response<TicketDetailsModel?>();

            try
            {
                if (ticketId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid ticket id.";
                    response.Data = null;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Title is required.";
                    response.Data = null;
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null || !user.IsActive)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "User not found or inactive.";
                    response.Data = null;
                    return response;
                }

                var ticket = await _ticketRepository.GetForAssignByIdAsync(ticketId);
                if (ticket == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket not found.";
                    response.Data = null;
                    return response;
                }

                if (ticket.Status == TicketStatus.Closed)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket is closed. You can't edit details.";
                    response.Data = null;
                    return response;
                }


                if (ticket.Status != TicketStatus.New)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "You are not allowed to edit this ticket  After Preformed.";
                    response.Data = null;
                    return response;

                }

                ticket.Title = title.Trim();
                ticket.Description = description?.Trim();
                ticket.UpdatedAt = DateTime.UtcNow;

                _ticketRepository.Update(ticket);
                await _ticketRepository.SaveChangesAsync();


                var fresh = await _ticketRepository.GetDetailsByIdAsync(ticketId);
                if (fresh == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Ticket updated but failed to load.";
                    response.Data = null;
                    return response;
                }

                var model = new TicketDetailsModel
                {
                    Id = fresh.Id,
                    Title = fresh.Title,
                    Description = fresh.Description,
                    Status = fresh.Status,
                    CreatedAt = fresh.CreatedAt,

                    ClientName = fresh.Client != null
                        ? (fresh.Client.FullName ?? fresh.Client.UserName)
                        : string.Empty,

                    AssignedEmployeeName = fresh.AssignedEmployee != null
                        ? (fresh.AssignedEmployee.FullName ?? fresh.AssignedEmployee.UserName)
                        : null,

                    ProductName = fresh.Product != null ? fresh.Product.Name : string.Empty,

                    Comments = fresh.Comments?
                        .OrderBy(c => c.CreatedAt)
                        .Select(c => new TicketCommentModel
                        {
                            Id = c.Id,
                            CommentText = c.CommentText,
                            CreatedAt = c.CreatedAt,
                            AuthorName = c.Author != null
                                ? (c.Author.FullName ?? c.Author.UserName)
                                : string.Empty,
                            IsFromSupportTeam = c.IsFromSupportTeam
                        })
                        .ToList() ?? new List<TicketCommentModel>()
                };

                response.Data = model;
                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateDetailsAsync failed. ticketId={TicketId}, userId={UserId}", ticketId, currentUserId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = null;
                return response;
            }
        }

    }
}

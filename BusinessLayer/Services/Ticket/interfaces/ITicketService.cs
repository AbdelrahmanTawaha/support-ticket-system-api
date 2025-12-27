using BusinessLayer.Models;
using BusinessLayer.Responses;

namespace BusinessLayer.Services.Ticket.interfaces
{
    public interface ITicketService
    {
        Task<PageResponse<List<TicketSummary>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? clientId,
            int? assignedEmployeeId,
            int? productId,
            string? status,
            string? searchTerm);
        Task<Response<TicketDetailsModel?>> GetByIdAsync(int id);

        Task<Response<TicketCommentModel?>> AddCommentAsync(int ticketId, int currentUserId, string commentText);
        Task<Response<bool>> AssignTicketAsync(int ticketId, int employeeId);


        Task<Response<bool>> MarkWaitingClientAsync(int ticketId, int currentUserId);
        Task<Response<bool>> ConfirmFixAsync(int ticketId, int currentUserId);
        Task<Response<bool>> RejectFixAsync(int ticketId, int currentUserId);
        Task<Response<int>> CreateClientTicketAsync(int clientId, string title, string? description, int productId);

        Task<Response<TicketDetailsModel?>> UpdateDetailsAsync(
           int ticketId,
           int currentUserId,
           string title,
           string? description
       );
    }

}

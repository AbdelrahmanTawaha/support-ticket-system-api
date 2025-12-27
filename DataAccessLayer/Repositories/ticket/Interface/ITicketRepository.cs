using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.ConfigurationsSetting.Enums;

namespace DataAccessLayer.Repositories.ticket.Interface
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<(List<Ticket> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? clientId = null,
            int? assignedEmployeeId = null,
            int? productId = null,
            TicketStatus? status = null,
            string? searchTerm = null);

        Task<Ticket?> GetForCommentByIdAsync(int id);
        Task AddCommentAsync(TicketComment comment);
        Task<bool> AssignTicketAsync(int ticketId, int employeeId);

        Task<Ticket?> GetDetailsByIdAsync(int id);

        Task<Ticket?> GetForAssignByIdAsync(int id);

        Task<List<(int Id, string Name, UserType UserType, bool IsActive, int TicketsCount)>>
            GetUsersWithTicketsCountAsync();

        Task<Dictionary<int, int>> GetTicketsCountByUsersAsync(List<int> userIds);
        Task<Dictionary<TicketStatus, int>> GetStatusCountsAsync();

        Task<List<(DateTime Day, int Count)>> GetTicketsTrendAsync(DateTime fromUtc, DateTime toUtc);

        Task<List<(int ProductId, string ProductName, int Count)>> GetTicketsByProductAsync(DateTime? fromUtc = null);
        Task<List<(int EmployeeId, string EmployeeName, string? ImageUrl, int AssignedCount, int ClosedCount)>>
            GetTopEmployeesAsync(int top = 5, DateTime? fromUtc = null);
        Task<Ticket?> GetForStatusChangeByIdAsync(int id);

    }
}

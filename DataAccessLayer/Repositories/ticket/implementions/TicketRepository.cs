using DataAccessLayer.ConfigurationsSetting;
using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.ConfigurationsSetting.Enums;
using DataAccessLayer.Repositories.ticket.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer.Repositories.ticket.implementions
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        private readonly ILogger<TicketRepository> _logger;

        public TicketRepository(AppDbContext context, ILogger<TicketRepository> logger)
            : base(context)
        {
            _logger = logger;
        }

        public async Task<(List<Ticket> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? clientId = null,
            int? assignedEmployeeId = null,
            int? productId = null,
            TicketStatus? status = null,
            string? searchTerm = null)
        {
            try
            {
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;

                var query = _dbSet
                    .Include(t => t.Client)
                    .Include(t => t.AssignedEmployee)
                    .Include(t => t.Product)
                    .AsNoTracking()
                    .OrderByDescending(t => t.CreatedAt)
                    .AsQueryable();

                if (assignedEmployeeId.HasValue)
                    query = query.Where(t => t.AssignedEmployeeId == assignedEmployeeId.Value);

                if (clientId.HasValue)
                    query = query.Where(t => t.ClientId == clientId.Value);

                if (productId.HasValue)
                    query = query.Where(t => t.ProductId == productId.Value);

                if (status.HasValue)
                    query = query.Where(t => t.Status == status.Value);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();
                    query = query.Where(t =>
                        t.Title.Contains(term) ||
                        t.Description.Contains(term));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPagedAsync failed.");
                throw;
            }
        }

        public async Task<Ticket?> GetDetailsByIdAsync(int id)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.Client)
                    .Include(t => t.AssignedEmployee)
                    .Include(t => t.Product)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.Author)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDetailsByIdAsync failed. id={Id}", id);
                throw;
            }
        }

        public async Task<Ticket?> GetForCommentByIdAsync(int id)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.Client)
                    .Include(t => t.AssignedEmployee)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.Author)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetForCommentByIdAsync failed. id={Id}", id);
                throw;
            }
        }

        public async Task AddCommentAsync(TicketComment comment)
        {
            try
            {
                _context.TicketComments.Add(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddCommentAsync failed. ticketId={TicketId}", comment?.TicketId);
                throw;
            }
        }

        public async Task<Ticket?> GetForAssignByIdAsync(int id)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.AssignedEmployee)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetForAssignByIdAsync failed. id={Id}", id);
                throw;
            }
        }



        public async Task<List<(int Id, string Name, UserType UserType, bool IsActive, int TicketsCount)>>
            GetUsersWithTicketsCountAsync()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.UserType == UserType.SupportEmployee || u.UserType == UserType.ExternalClient)
                    .Select(u => new
                    {
                        u.Id,
                        Name = u.FullName ?? u.UserName ?? "",
                        u.UserType,
                        u.IsActive,
                        TicketsCount =
                            _context.Tickets.Count(t =>
                                (u.UserType == UserType.SupportEmployee && t.AssignedEmployeeId == u.Id) ||
                                (u.UserType == UserType.ExternalClient && t.ClientId == u.Id)
                            )
                    })
                    .ToListAsync();

                return users
                    .Select(x => (x.Id, x.Name, x.UserType, x.IsActive, x.TicketsCount))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUsersWithTicketsCountAsync failed.");
                throw;
            }
        }

        public async Task<Dictionary<int, int>> GetTicketsCountByUsersAsync(List<int> userIds)
        {
            try
            {
                var employeeCounts = await _context.Tickets
                    .Where(t => t.AssignedEmployeeId != null && userIds.Contains(t.AssignedEmployeeId.Value))
                    .GroupBy(t => t.AssignedEmployeeId.Value)
                    .Select(g => new { UserId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var clientCounts = await _context.Tickets
                    .Where(t => userIds.Contains(t.ClientId))
                    .GroupBy(t => t.ClientId)
                    .Select(g => new { UserId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var dict = new Dictionary<int, int>();

                foreach (var x in employeeCounts)
                    dict[x.UserId] = x.Count;

                foreach (var x in clientCounts)
                {
                    if (dict.ContainsKey(x.UserId))
                        dict[x.UserId] += x.Count;
                    else
                        dict[x.UserId] = x.Count;
                }

                return dict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTicketsCountByUsersAsync failed.");
                throw;
            }
        }

        public async Task<bool> AssignTicketAsync(int ticketId, int employeeId)
        {
            try
            {
                var ticket = await GetForAssignByIdAsync(ticketId);
                if (ticket == null) return false;

                ticket.AssignedEmployeeId = employeeId;
                ticket.UpdatedAt = DateTime.UtcNow;

                Update(ticket);
                await SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AssignTicketAsync failed. ticketId={TicketId}, employeeId={EmployeeId}", ticketId, employeeId);
                throw;
            }
        }

        public async Task<Dictionary<TicketStatus, int>> GetStatusCountsAsync()
        {
            try
            {
                var data = await _context.Tickets
                    .AsNoTracking()
                    .GroupBy(t => t.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var dict = Enum.GetValues(typeof(TicketStatus))
                    .Cast<TicketStatus>()
                    .ToDictionary(s => s, _ => 0);

                foreach (var x in data)
                    dict[x.Status] = x.Count;

                return dict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStatusCountsAsync failed.");
                throw;
            }
        }

        public async Task<List<(DateTime Day, int Count)>> GetTicketsTrendAsync(DateTime fromUtc, DateTime toUtc)
        {
            try
            {
                var data = await _context.Tickets
                    .AsNoTracking()
                    .Where(t => t.CreatedAt >= fromUtc && t.CreatedAt <= toUtc)
                    .GroupBy(t => t.CreatedAt.Date)
                    .Select(g => new { Day = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Day)
                    .ToListAsync();

                return data.Select(x => (x.Day, x.Count)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTicketsTrendAsync failed.");
                throw;
            }
        }

        public async Task<List<(int ProductId, string ProductName, int Count)>> GetTicketsByProductAsync(DateTime? fromUtc = null)
        {
            try
            {
                var q = _context.Tickets.AsNoTracking().AsQueryable();

                if (fromUtc.HasValue)
                    q = q.Where(t => t.CreatedAt >= fromUtc.Value);

                var data = await q
                    .Include(t => t.Product)
                    .GroupBy(t => new { t.ProductId, Name = t.Product.Name })
                    .Select(g => new
                    {
                        g.Key.ProductId,
                        ProductName = g.Key.Name,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                return data.Select(x => (x.ProductId, x.ProductName, x.Count)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTicketsByProductAsync failed.");
                throw;
            }
        }

        public async Task<List<(int EmployeeId, string EmployeeName, string? ImageUrl, int AssignedCount, int ClosedCount)>>
 GetTopEmployeesAsync(int top = 5, DateTime? fromUtc = null)
        {
            try
            {
                var q = _context.Tickets.AsNoTracking().AsQueryable();

                if (fromUtc.HasValue)
                    q = q.Where(t => t.CreatedAt >= fromUtc.Value);

                q = q.Where(t => t.AssignedEmployeeId != null);

                var data = await q
                    .GroupBy(t => t.AssignedEmployeeId!.Value)
                    .Select(g => new
                    {
                        EmployeeId = g.Key,
                        AssignedCount = g.Count(),
                        ClosedCount = g.Count(x => x.Status == TicketStatus.Closed)

                    })
                    .OrderByDescending(x => x.ClosedCount)
                    .ThenByDescending(x => x.AssignedCount)
                    .Take(top)
                    .ToListAsync();

                var empIds = data.Select(x => x.EmployeeId).ToList();

                var employees = await _context.Users
                    .AsNoTracking()
                    .Where(u => empIds.Contains(u.Id))
                    .Select(u => new
                    {
                        u.Id,
                        Name = u.FullName ?? u.UserName ?? "",
                        u.ImageUrl
                    })
                    .ToListAsync();

                var empDict = employees.ToDictionary(x => x.Id, x => new { x.Name, x.ImageUrl });

                return data.Select(x =>
                {
                    var found = empDict.TryGetValue(x.EmployeeId, out var e);

                    return (//lambda
                        x.EmployeeId,
                        found ? e!.Name : $"Employee {x.EmployeeId}",
                        found ? e!.ImageUrl : null,
                        x.AssignedCount,
                        x.ClosedCount

                    );
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTopEmployeesAsync failed.");
                throw;
            }
        }


        public async Task<Ticket?> GetForStatusChangeByIdAsync(int id)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.Client)
                    .Include(t => t.AssignedEmployee)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetForStatusChangeByIdAsync failed. id={Id}", id);
                throw;
            }
        }
    }

}

using BusinessLayer.Models.Dashboard;
using BusinessLayer.Responses;
using DataAccessLayer.Repositories.ticket.Interface;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Services.DashboardService
{
    public class DashboardService : IDashboardService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            ITicketRepository ticketRepository,
            ILogger<DashboardService> logger)
        {
            _ticketRepository = ticketRepository;
            _logger = logger;
        }

        public async Task<Response<DashboardSummaryModel>> GetManagerSummaryAsync()
        {
            var response = new Response<DashboardSummaryModel>();

            try
            {
                var now = DateTime.UtcNow;
                var from30 = now.AddDays(-30);


                var statusCounts = await _ticketRepository.GetStatusCountsAsync();
                var trend = await _ticketRepository.GetTicketsTrendAsync(from30, now);
                var topEmployees = await _ticketRepository.GetTopEmployeesAsync(5, from30);
                var byProduct = await _ticketRepository.GetTicketsByProductAsync(from30);

                var topEmployee = await _ticketRepository.GetTopEmployeesAsync(5, from30);

                var model = new DashboardSummaryModel
                {
                    StatusCounts = statusCounts,

                    Trend = trend.Select(x => new TicketsTrendPointModel
                    {
                        Day = x.Day,
                        Count = x.Count
                    }).ToList(),

                    TopEmployees = topEmployees.Select(x => new TopEmployeeModel
                    {
                        Id = x.EmployeeId,
                        Name = x.EmployeeName,
                        AssignedCount = x.AssignedCount,
                        ClosedCount = x.ClosedCount,
                        ImageUrl = x.ImageUrl
                    }).ToList(),

                    TicketsByProduct = byProduct.Select(x => new TicketsByProductModel
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        Count = x.Count
                    }).ToList()
                };

                response.Data = model;
                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetManagerSummaryAsync failed.");

                response.Data = null;
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
            }

            return response;
        }
    }
}

namespace SupportTicketsAPI.DTOs.Dashboard
{

    public class DashboardSummaryDto
    {
        public Dictionary<string, int> StatusCounts { get; set; } = new();

        public List<TrendPointDto> Trend { get; set; } = new();

        public List<TopEmployeeDto> TopEmployees { get; set; } = new();

        public List<TicketsByProductDto> TicketsByProduct { get; set; } = new();
    }
}



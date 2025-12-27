using DataAccessLayer.ConfigurationsSetting.Enums;

namespace BusinessLayer.Models.Dashboard
{
    public class DashboardSummaryModel
    {
        public Dictionary<TicketStatus, int> StatusCounts { get; set; } = new();

        public List<TicketsTrendPointModel> Trend { get; set; } = new();

        public List<TopEmployeeModel> TopEmployees { get; set; } = new();

        public List<TicketsByProductModel> TicketsByProduct { get; set; } = new();
    }
}

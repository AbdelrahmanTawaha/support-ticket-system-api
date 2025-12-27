using BusinessLayer.Models.Dashboard;
using BusinessLayer.Responses;
namespace BusinessLayer.Services.DashboardService
{

    public interface IDashboardService
    {
        Task<Response<DashboardSummaryModel>> GetManagerSummaryAsync();
    }
}

using BusinessLayer.Responses;
using BusinessLayer.Services.DashboardService;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SupportTicketsAPI.DTOs.Dashboard;

using SupportTicketsAPI.Services;

namespace SupportTicketsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Manager)]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService service, ILogger<DashboardController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("manager-summary")]
        public async Task<ActionResult<Response<DashboardSummaryDto>>> GetManagerSummary()
        {
            var api = new Response<DashboardSummaryDto>();

            try
            {
                var result = await _service.GetManagerSummaryAsync();

                if (result == null)
                {
                    _logger.LogWarning("Dashboard service returned null result.");

                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Unexpected null response from business layer.";
                    api.Data = null;
                    return StatusCode(StatusCodes.Status500InternalServerError, api);
                }

                if (result.ErrorCode != ErrorCode.Success || result.Data == null)
                {
                    _logger.LogInformation("Manager summary failed. ErrorCode={ErrorCode}, Msg={Msg}",
                        result.ErrorCode, result.MsgError);

                    api.ErrorCode = result.ErrorCode;
                    api.MsgError = result.MsgError;
                    api.Data = null;
                    return BadRequest(api);
                }

                var dto = new DashboardSummaryDto
                {
                    StatusCounts = result.Data.StatusCounts.ToDictionary(
                        k => k.Key.ToString(),
                        v => v.Value
                    ),

                    Trend = result.Data.Trend.Select(x => new TrendPointDto
                    {
                        Day = x.Day,
                        Count = x.Count
                    }).ToList(),

                    TopEmployees = result.Data.TopEmployees.Select(x => new TopEmployeeDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        AssignedCount = x.AssignedCount,
                        ClosedCount = x.ClosedCount,
                        ResolvedCount = x.ResolvedCount,
                        ImageUrl = x.ImageUrl

                    }).ToList(),

                    TicketsByProduct = result.Data.TicketsByProduct.Select(x => new TicketsByProductDto
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        Count = x.Count
                    }).ToList()
                };

                api.ErrorCode = ErrorCode.Success;
                api.MsgError = ErrorCode.Success.ToString();
                api.Data = dto;

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetManagerSummary exception.");

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SupportTicketsAPI.Services.AiReport;
using SupportTicketsAPI.Services.AiReport.dto;

namespace SupportTicketsAPI.Controllers
{
    [ApiController]
    [Route("api/admin/ai")]
    [Authorize(Roles = "SupportManager")]
    public class AdminAiController : ControllerBase
    {
        private readonly IAiSqlGenerator _generator;
        private readonly IAiSqlLightValidator _validator;
        private readonly IAiReportExecutor _executor;

        public AdminAiController(
            IAiSqlGenerator generator,
            IAiSqlLightValidator validator,
            IAiReportExecutor executor)
        {
            _generator = generator;
            _validator = validator;
            _executor = executor;
        }

        [HttpPost("report")]
        public async Task<ActionResult<AiSqlResponse>> Report([FromBody] AiSqlRequest req, CancellationToken ct)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Prompt))
                return BadRequest("Prompt is required.");

            var plan = await _generator.GeneratePlanAsync(req.Prompt, ct);


            var normalized = SqlFragmentNormalizer.Normalize(plan.Fragment);
            plan = plan with { Fragment = normalized };


            try
            {
                _validator.Validate(plan);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    error = ex.Message,
                    view = plan.View,
                    fragment = plan.Fragment
                });
            }

            object data;
            try
            {
                data = await _executor.ExecuteAsync(plan, ct);
            }
            catch (SqlException ex)
            {
                return BadRequest(new
                {
                    error = "SQL execution failed.",
                    details = ex.Message,
                    view = plan.View,
                    fragment = plan.Fragment
                });
            }

            var rowsCount = data is System.Collections.ICollection col ? col.Count : 0;
            var (isFallback, warning) = AiQueryQuality.Evaluate(plan.Fragment, rowsCount);

            return Ok(new AiSqlResponse
            {
                View = plan.View,
                Fragment = plan.Fragment,
                Data = data,
                IsFallback = isFallback,
                Warning = warning
            });
        }


    }
}

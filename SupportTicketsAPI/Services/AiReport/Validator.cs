using Microsoft.Extensions.Options;
using SupportTicketsAPI.Services.AiReport;
using SupportTicketsAPI.Services.AiReport.dto;

public interface IAiSqlLightValidator
{
    void Validate(AiQueryPlan plan);
}

public class AiSqlLightValidator : IAiSqlLightValidator
{
    private readonly AiSqlOptions _opt;

    public AiSqlLightValidator(IOptions<AiSqlOptions> opt)
    {
        _opt = opt.Value;
    }

    public void Validate(AiQueryPlan plan)
    {
        if (plan == null)
            throw new InvalidOperationException("Plan is required.");


        if (string.IsNullOrWhiteSpace(plan.View))
            throw new InvalidOperationException("View is required.");

        var view = plan.View.Trim();

        var allowed = _opt.AllowedViews.Any(v =>
            string.Equals(v, view, StringComparison.OrdinalIgnoreCase));

        if (!allowed)
            throw new InvalidOperationException($"View not allowed: {view}");


        var fragment = plan.Fragment ?? "";
        var s = fragment.Trim();


        if (string.IsNullOrEmpty(s))
            return;


        if (s.Contains(";"))
            throw new InvalidOperationException("Semicolons are not allowed.");

        var lower = s.ToLowerInvariant();


        if (!lower.StartsWith("where ") && !lower.StartsWith("order by "))
            throw new InvalidOperationException("Fragment must start with WHERE or ORDER BY or be empty.");


        if (lower.Contains("vw_") || lower.Contains("dbo."))
            throw new InvalidOperationException("Fragment must not reference tables/views.");


        if (lower.Contains("/*") || lower.Contains("--"))
            throw new InvalidOperationException("Comments are not allowed.");


        string[] forbidden =
        {
            "select ", " from ", " join ",
            " insert ", " update ", " delete ",
            " drop ", " alter ", " create ",
            " exec ", " execute ", " with ",
            " union ", " intersect ", " except "
        };

        foreach (var k in forbidden)
        {
            if (lower.Contains(k))
                throw new InvalidOperationException($"Forbidden keyword in fragment: {k.Trim()}");
        }


        if (lower.Contains("status = '") || lower.Contains("status='") ||
            lower.Contains("status = \"") || lower.Contains("status=\""))
        {
            throw new InvalidOperationException("Status must be numeric (0..4), not text like 'Open'.");
        }


        if (lower.StartsWith("where and "))
            throw new InvalidOperationException("Invalid fragment: 'WHERE AND ...'.");
    }
}

using DataAccessLayer.ConfigurationsSetting;
using DataAccessLayer.ConfigurationsSetting.AiViews;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SupportTicketsAPI.Services.AiReport;
using SupportTicketsAPI.Services.AiReport.dto;

public interface IAiReportExecutor
{
    Task<object> ExecuteAsync(AiQueryPlan plan, CancellationToken ct = default);
}

public class AiReportExecutor : IAiReportExecutor
{
    private readonly AppDbContext _db;
    private readonly AiSqlOptions _ai;

    public AiReportExecutor(AppDbContext db, IOptions<AiSqlOptions> aiOpt)
    {
        _db = db;
        _ai = aiOpt.Value;
    }

    public async Task<object> ExecuteAsync(AiQueryPlan plan, CancellationToken ct = default)
    {
        if (plan == null) throw new InvalidOperationException("Plan is required.");

        var view = plan.View.Trim();


        var allowed = _ai.AllowedViews.Any(v => string.Equals(v, view, StringComparison.OrdinalIgnoreCase));
        if (!allowed)
            throw new InvalidOperationException($"View not allowed: {view}");

        var frag = plan.Fragment?.Trim();
        var baseSql = $"SELECT TOP ({_ai.DefaultTop}) * FROM dbo.{view}";
        var finalSql = string.IsNullOrEmpty(frag) ? baseSql : $"{baseSql} {frag}";

        var lowerView = view.ToLowerInvariant();

        return lowerView switch
        {
            "vw_users_ai_safe" => await _db.Set<UserAiSafeRow>()
                .FromSqlRaw(finalSql).AsNoTracking().ToListAsync(ct),

            "vw_tickets_ai_report" => await _db.Set<TicketAiReportRow>()
                .FromSqlRaw(finalSql).AsNoTracking().ToListAsync(ct),

            _ => throw new InvalidOperationException($"Unsupported view: {view}")
        };
    }
}

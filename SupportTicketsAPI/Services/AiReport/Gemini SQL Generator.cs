using System.Text.Json;
using Microsoft.Extensions.Options;
using SupportTicketsAPI.Services.AiReport;
using SupportTicketsAPI.Services.AiReport.dto;

public interface IAiSqlGenerator
{

    Task<AiQueryPlan> GeneratePlanAsync(string prompt, CancellationToken ct = default);
}

public class GeminiSqlGenerator : IAiSqlGenerator
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _gemini;
    private readonly AiSqlOptions _ai;

    public GeminiSqlGenerator(HttpClient http,
        IOptions<GeminiOptions> geminiOpt,
        IOptions<AiSqlOptions> aiOpt)
    {
        _http = http;
        _gemini = geminiOpt.Value;
        _ai = aiOpt.Value;
    }

    public async Task<AiQueryPlan> GeneratePlanAsync(string prompt, CancellationToken ct = default)
    {
        var allowedViews = string.Join(", ", _ai.AllowedViews);

        var systemPrompt = $@"
You output ONLY JSON. No markdown. No extra text.

Choose ONE view from:
[{allowedViews}]

Return JSON exactly like:
{{ ""view"": ""vw_Tickets_AI_Report"", ""fragment"": ""WHERE Status IN (0,1,2) ORDER BY CreatedAt DESC"" }}

Fragment rules:
- Fragment can be empty """" OR must start with WHERE or ORDER BY.
- Do NOT use any of these: SELECT, FROM, JOIN, UNION, INSERT, UPDATE, DELETE, DROP, ALTER, EXEC, WITH, /*, --, ;
- Fragment must NOT mention dbo. or vw_ or any table/view name.

Tickets Status is INT:
0 New, 1 InProgress, 2 WaitingClient, 3 Resolved, 4 Closed
Open tickets = Status IN (0,1,2)
Closed tickets = Status = 4
Resolved (not closed) = Status = 3
DO NOT compare Status to text like 'Open' or 'Closed'.

Examples:
User: show open tickets
Output: {{ ""view"": ""vw_Tickets_AI_Report"", ""fragment"": ""WHERE Status IN (0,1,2) ORDER BY CreatedAt DESC"" }}

User: show active users
Output: {{ ""view"": ""vw_Users_AI_Safe"", ""fragment"": ""WHERE IsActive = 1 ORDER BY CreatedAt DESC"" }}

If user request is unclear, return:
{{ ""view"": ""vw_Tickets_AI_Report"", ""fragment"": ""ORDER BY CreatedAt DESC"" }}
";

        var url = $"{_gemini.BaseUrl}/v1beta/models/{_gemini.Model}:generateContent?key={_gemini.ApiKey}";

        var body = new
        {
            contents = new object[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = systemPrompt + "\nUSER REQUEST:\n" + prompt }
                    }
                }
            }
        };

        var resp = await _http.PostAsJsonAsync(url, body, ct);
        if (!resp.IsSuccessStatusCode)
        {

            var caption = await resp.Content.ReadAsStringAsync(ct);

            throw new HttpRequestException(
                $"AI request failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {caption}"
            );
        }

        var json = await resp.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct);
        var text = json?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text ?? "{}";

        var cleaned = SqlFragmentNormalizer.ExtractJson(text);


        var plan = JsonSerializer.Deserialize<AiQueryPlan>(cleaned, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new AiQueryPlan("vw_Tickets_AI_Report", "ORDER BY CreatedAt DESC");

        return plan;
    }

    private class GeminiResponse { public List<Candidate>? candidates { get; set; } }
    private class Candidate { public Content? content { get; set; } }
    private class Content { public List<Part>? parts { get; set; } }
    private class Part { public string? text { get; set; } }
}
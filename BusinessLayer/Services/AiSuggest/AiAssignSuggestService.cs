using System.Net.Http.Json;
using System.Text.Json;

using BusinessLayer.Models;
using BusinessLayer.Services.AiSuggest;
using DataAccessLayer.ConfigurationsSetting.Enums;
using DataAccessLayer.Repositories.ticket.Interface;
using Microsoft.Extensions.Configuration;

namespace BusinessLayer.Services.AiAssignSuggest
{
    public class AiAssignSuggestService : IAiAssignSuggestService
    {
        private readonly HttpClient _http;
        private readonly ITicketRepository _ticketRepo;
        private readonly IConfiguration _config;

        public AiAssignSuggestService(HttpClient http, ITicketRepository ticketRepo, IConfiguration config)
        {
            _http = http;
            _ticketRepo = ticketRepo;
            _config = config;
        }

        public async Task<AiAssignSuggestionResult> SuggestAsync(int ticketId, CancellationToken ct = default)
        {

            var ticket = await _ticketRepo.GetDetailsByIdAsync(ticketId);
            if (ticket == null)
            {
                return new AiAssignSuggestionResult
                {
                    IsFallback = true,
                    Warning = "Ticket not found."
                };
            }

            if (ticket.Status == TicketStatus.Closed)
            {
                return new AiAssignSuggestionResult
                {
                    IsFallback = true,
                    Warning = "Ticket is closed."
                };
            }


            var candidates = await _ticketRepo.GetTopEmployeesAsync(top: 8);

            if (candidates.Count == 0)
            {
                return new AiAssignSuggestionResult
                {
                    IsFallback = true,
                    Warning = "No employee candidates available."
                };
            }


            var systemPrompt = """
You are a support manager assistant.

Return ONLY valid JSON. No markdown. No code fences. No explanations.

JSON schema:
{
  "suggestedEmployeeId": 123,
  "confidence": 0.0,
  "reason": "short reason",
  "warning": null
}

Rules:
- suggestedEmployeeId MUST be one of the candidate IDs provided.
- confidence is between 0.0 and 1.0
- reason must be concise and practical for assigning the ticket.
- If uncertain, set suggestedEmployeeId to null and add warning.
""";

            var lastComments = ticket.Comments?
                .OrderByDescending(c => c.CreatedAt)
                .Take(3)
                .Select(c => $"- ({c.CreatedAt:u}) {c.CommentText}")
                .ToList() ?? new List<string>();

            var candidatesText = string.Join("\n", candidates.Select(c =>
                $"- Id={c.EmployeeId}, Name={c.EmployeeName}, Assigned={c.AssignedCount}, Closed={c.ClosedCount}"
            ));

            var userPrompt = $"""
TICKET:
- Id: {ticket.Id}
- Title: {ticket.Title}
- Description: {ticket.Description}
- Product: {ticket.Product?.Name}

LAST COMMENTS (if any):
{(lastComments.Count == 0 ? "- none" : string.Join("\n", lastComments))}

CANDIDATE EMPLOYEES (choose ONLY from these IDs):
{candidatesText}
""";


            var body = new
            {
                contents = new object[]
                {
                    new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = systemPrompt + "\nUSER REQUEST:\n" + userPrompt }
                        }
                    }
                }
            };

            var url = BuildGeminiUrl();
            if (url == null)
            {
                return new AiAssignSuggestionResult
                {
                    IsFallback = true,
                    Warning = "Gemini configuration is missing (BaseUrl/ApiKey/Model)."
                };
            }


            var resp = await _http.PostAsJsonAsync(url, body, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var caption = await resp.Content.ReadAsStringAsync(ct);
                return new AiAssignSuggestionResult
                {
                    IsFallback = true,
                    Warning = $"AI call failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. {caption}"
                };
            }

            var gemini = await resp.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct);

            var parts = gemini?.candidates?.FirstOrDefault()?.content?.parts;
            var text = (parts == null || parts.Count == 0)
                ? "{}"
                : string.Concat(parts.Select(p => p?.text ?? ""));


            var cleaned = JsonNormalizer.ExtractJson(text);


            AiAssignSuggestModel? model;
            try
            {
                model = JsonSerializer.Deserialize<AiAssignSuggestModel>(cleaned, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                return new AiAssignSuggestionResult
                {
                    IsFallback = true,
                    Warning = "Failed to parse AI JSON: " + ex.Message,
                    RawJson = cleaned
                };
            }

            var suggestedId = model?.SuggestedEmployeeId;


            var found = suggestedId.HasValue
                ? candidates.FirstOrDefault(x => x.EmployeeId == suggestedId.Value)
                : default;

            if (!suggestedId.HasValue || found.EmployeeId == 0)
            {
                var fallback = candidates.First();

                return new AiAssignSuggestionResult
                {
                    IsFallback = true,
                    Warning = model?.Warning ?? "AI did not return a valid candidate id.",
                    Confidence = Clamp01(model?.Confidence ?? 0.0),
                    Reason = model?.Reason ?? "Fallback selection based on top employees list.",
                    SuggestedEmployeeId = fallback.EmployeeId,
                    SuggestedEmployeeName = fallback.EmployeeName,
                    RawJson = cleaned
                };
            }

            return new AiAssignSuggestionResult
            {
                SuggestedEmployeeId = found.EmployeeId,
                SuggestedEmployeeName = found.EmployeeName,
                Confidence = Clamp01(model?.Confidence ?? 0.0),
                Reason = model?.Reason ?? "",
                Warning = model?.Warning,
                IsFallback = false,
                RawJson = cleaned
            };
        }

        private string? BuildGeminiUrl()
        {
            var baseUrl = _config["Gemini:BaseUrl"];
            var apiKey = _config["Gemini:ApiKey"];
            var model = _config["Gemini:Model"];

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(model))
                return null;



            return $"{baseUrl.TrimEnd('/')}/v1beta/models/{model}:generateContent?key={apiKey}";
        }

        private static double Clamp01(double v)
        {
            if (v < 0.0) return 0.0;
            if (v > 1.0) return 1.0;
            return v;
        }


        private class GeminiResponse { public List<Candidate>? candidates { get; set; } }
        private class Candidate { public Content? content { get; set; } }
        private class Content { public List<Part>? parts { get; set; } }
        private class Part { public string? text { get; set; } }
    }
}

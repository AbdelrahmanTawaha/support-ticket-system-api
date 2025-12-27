namespace SupportTicketsAPI.Services.AiReport.dto
{
    public record AiSqlRequest(string Prompt);

    public class AiSqlResponse
    {
        public string View { get; set; } = "";
        public string Fragment { get; set; } = "";
        public object? Data { get; set; }

        public bool IsFallback { get; set; }
        public string? Warning { get; set; }
    }


    public record AiQueryPlan(string View, string Fragment);
}

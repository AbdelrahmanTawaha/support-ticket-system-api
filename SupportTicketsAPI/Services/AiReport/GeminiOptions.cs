namespace SupportTicketsAPI.Services.AiReport
{
    public class GeminiOptions
    {
        public string BaseUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "";
    }

    public class AiSqlOptions
    {
        public List<string> AllowedViews { get; set; } = new();
        public int DefaultTop { get; set; } = 50;
    }
}

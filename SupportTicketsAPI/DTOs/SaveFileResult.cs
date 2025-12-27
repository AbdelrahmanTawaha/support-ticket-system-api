using BusinessLayer.Responses;

namespace SupportTicketsAPI.DTOs
{
    public class SaveFileResult
    {
        public ErrorCode ErrorCode { get; set; }
        public string? Msg { get; set; }
        public string? Path { get; set; }
    }
}

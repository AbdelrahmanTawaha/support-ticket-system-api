namespace SupportTicketsAPI.DTOs
{
    public class AiAssignSuggestResponseDto
    {
        public int? SuggestedEmployeeId { get; set; }
        public string? SuggestedEmployeeName { get; set; }

        public double Confidence { get; set; }
        public string Reason { get; set; } = "";

        public bool IsFallback { get; set; }
        public string? Warning { get; set; }
    }
}

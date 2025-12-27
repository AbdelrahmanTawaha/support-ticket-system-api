

namespace BusinessLayer.Models
{
    public class AiAssignSuggestionResult
    {
        public int? SuggestedEmployeeId { get; set; }
        public string? SuggestedEmployeeName { get; set; }
        public double Confidence { get; set; }
        public string Reason { get; set; } = "";
        public bool IsFallback { get; set; }
        public string? Warning { get; set; }
        public string? RawJson { get; set; }
    }
}

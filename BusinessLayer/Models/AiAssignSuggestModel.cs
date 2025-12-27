namespace BusinessLayer.Models
{
    public class AiAssignSuggestModel
    {
        public int? SuggestedEmployeeId { get; set; }
        public double Confidence { get; set; }
        public string? Reason { get; set; }
        public string? Warning { get; set; }
    }
}

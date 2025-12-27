namespace BusinessLayer.Models
{
    public class ForgotPasswordResultModel
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}

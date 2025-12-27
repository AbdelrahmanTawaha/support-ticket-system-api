namespace SupportTicketsAPI.Services.Email
{
    public class SmtpSettings
    {
        public string User { get; set; } = null!;
        public string From { get; set; } = null!;
        public string FromName { get; set; } = "T2 Company";
        public string Pass { get; set; } = null!;
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
    }
}

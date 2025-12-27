namespace SupportTicketsAPI.DTOs
{
    public class RegisterClientFormDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";

        public string CompanyName { get; set; } = "";
        public string? CompanyAddress { get; set; }
        public string? VatNumber { get; set; }
        public string? PreferredLanguage { get; set; } = "en";

        public IFormFile? ProfileImage { get; set; }
    }
}

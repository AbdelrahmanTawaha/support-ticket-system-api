namespace BusinessLayer.Models
{
    public class RegisterClientModel
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? UserName { get; set; }
        public string Password { get; set; } = null!;


        public string CompanyName { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? VatNumber { get; set; }
        public string? PreferredLanguage { get; set; }
    }
}

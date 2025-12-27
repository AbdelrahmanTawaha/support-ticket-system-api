namespace DataAccessLayer.ConfigurationsSetting.Entity
{
    public class ClientProfile
    {
        public int UserId { get; set; }

        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? VatNumber { get; set; }
        public string? PreferredLanguage { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedOn { get; set; }

        public User User { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.ConfigurationsSetting.AiViews
{
    [Keyless]

    public class ClientProfileAiSafeRow
    {
        public int UserId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? VatNumber { get; set; }
        public string? PreferredLanguage { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }
    }
}

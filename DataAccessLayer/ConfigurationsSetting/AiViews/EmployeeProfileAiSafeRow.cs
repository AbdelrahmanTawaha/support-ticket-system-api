using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.ConfigurationsSetting.AiViews
{
    [Keyless]

    public class EmployeeProfileAiSafeRow
    {
        public int UserId { get; set; }
        public string? EmployeeCode { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public string? JobTitle { get; set; }
        public int? ManagerUserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }
    }
}

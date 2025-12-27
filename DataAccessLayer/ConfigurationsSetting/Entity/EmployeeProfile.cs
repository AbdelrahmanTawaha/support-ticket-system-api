using DataAccessLayer.ConfigurationsSetting.Enums;

namespace DataAccessLayer.ConfigurationsSetting.Entity
{
    public class EmployeeProfile
    {

        public int UserId { get; set; }

        public string EmployeeCode { get; set; }
        public DateTime HireDate { get; set; }
        public decimal? Salary { get; set; }


        public EmployeeJobTitle JobTitle { get; set; }


        public int? ManagerUserId { get; set; }
        public User? ManagerUser { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedOn { get; set; }

        public User User { get; set; }
    }
}

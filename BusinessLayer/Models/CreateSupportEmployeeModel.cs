using DataAccessLayer.ConfigurationsSetting.Enums;

namespace BusinessLayer.Models
{
    public class CreateSupportEmployeeModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }


        public string? UserName { get; set; }
        public string Password { get; set; } = string.Empty;


        public string? EmployeeCode { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal? Salary { get; set; }
        public EmployeeJobTitle? JobTitle { get; set; }
    }
}

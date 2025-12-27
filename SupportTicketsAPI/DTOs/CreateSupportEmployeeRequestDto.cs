using System.ComponentModel.DataAnnotations;
using DataAccessLayer.ConfigurationsSetting.Enums;

namespace SupportTicketsAPI.DTOs
{

    public class CreateSupportEmployeeRequestDto
    {

        [Required, MinLength(3)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ImageUrl { get; set; }

        public string? UserName { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;


        public string? EmployeeCode { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal? Salary { get; set; }

        // matches your enum EmployeeJobTitle:
        // 0 = SupportManager, 1 = EmployeeSupport
        public EmployeeJobTitle? JobTitle { get; set; }
    }


}

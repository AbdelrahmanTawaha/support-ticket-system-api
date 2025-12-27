using DataAccessLayer.ConfigurationsSetting.Enums;

namespace BusinessLayer.Models
{
    public class UserEditModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ImageUrl { get; set; }


        public UserType UserType { get; set; }
        public bool IsActive { get; set; }
    }
}

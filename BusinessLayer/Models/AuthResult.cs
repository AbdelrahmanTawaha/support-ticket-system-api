using DataAccessLayer.ConfigurationsSetting.Enums;

namespace BusinessLayer.Models
{
    public class AuthResult
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public UserType UserType { get; set; }
    }
}

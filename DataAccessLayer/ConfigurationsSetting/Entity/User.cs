using DataAccessLayer.ConfigurationsSetting.Enums;

namespace DataAccessLayer.ConfigurationsSetting.Entity
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ImageUrl { get; set; }

        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; } = true;
        public string? PasswordResetCodeHash { get; set; }
        public DateTime? PasswordResetCodeExpiresAt { get; set; }
        public int PasswordResetAttempts { get; set; } = 0;
        public DateTime? PasswordResetLastSentAt { get; set; }

        public UserType UserType { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }




        public ICollection<Ticket> CreatedTickets { get; set; }
        public ICollection<Ticket> AssignedTickets { get; set; }

        public ICollection<TicketComment> Comments { get; set; }


        public EmployeeProfile? EmployeeProfile { get; set; }

        public ClientProfile? ClientProfile { get; set; }
        public ICollection<EmployeeProfile> ManagedEmployees { get; set; }

    }
}

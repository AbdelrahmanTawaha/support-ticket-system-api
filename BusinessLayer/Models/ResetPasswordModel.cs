namespace BusinessLayer.Models
{
    public class ResetPasswordModel
    {
        public string UserNameOrEmail { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}

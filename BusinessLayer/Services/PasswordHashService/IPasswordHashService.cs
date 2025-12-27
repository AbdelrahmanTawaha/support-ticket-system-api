namespace BusinessLayer.Services.PasswordHashService
{
    public interface IPasswordHashService
    {
        string Hash(string password);
        bool Verify(string hashedPassword, string providedPassword);
    }
}

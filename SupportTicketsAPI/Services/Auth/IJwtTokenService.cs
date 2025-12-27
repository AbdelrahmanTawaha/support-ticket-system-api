using BusinessLayer.Models;

namespace SupportTicketsAPI.Services.Auth
{
    public interface IJwtTokenService
    {
        JwtTokenResult GenerateToken(AuthResult authResult);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLayer.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace SupportTicketsAPI.Services.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public JwtTokenResult GenerateToken(AuthResult authResult)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, authResult.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, authResult.UserName),
                    new Claim(ClaimTypes.NameIdentifier, authResult.UserId.ToString()),
                    new Claim(ClaimTypes.Name, authResult.FullName ?? authResult.UserName),
                    new Claim(ClaimTypes.Role,authResult.UserType.ToString()),
                };



                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes);

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return new JwtTokenResult
                {
                    Token = tokenString,
                    ExpiresAt = expires
                };
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException("Failed to generate JWT token.", ex);
            }
        }
    }
}



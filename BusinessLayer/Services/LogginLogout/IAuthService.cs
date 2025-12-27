using BusinessLayer.Models;
using BusinessLayer.Responses;
namespace BusinessLayer.Services.LogginLogout
{
    public interface IAuthService
    {
        Task<Response<AuthResult>> LoginAsync(string userNameOrEmail, string password);
        Task<Response<int>> RegisterClientAsync(RegisterClientModel model);

        Task<Response<bool>> ResetPasswordAsync(ResetPasswordModel model);
        Task<Response<ForgotPasswordResultModel?>> ForgotPasswordAsync(ForgotPasswordRequestModel model);

    }
}

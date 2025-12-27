
using System.Security.Cryptography;
using BusinessLayer.Models;
using BusinessLayer.Responses;
using BusinessLayer.Services.PasswordHashService;
using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.ConfigurationsSetting.Enums;
using DataAccessLayer.Repositories.user;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Services.LogginLogout
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHashService _passwordHash;
        private readonly IClientProfileRepository _clientProfileRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHashService passwordHash,
            IClientProfileRepository clientProfileRepository,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _passwordHash = passwordHash;
            _clientProfileRepository = clientProfileRepository;
            _logger = logger;
        }

        public async Task<Response<AuthResult>> LoginAsync(string userNameOrEmail, string password)
        {
            var response = new Response<AuthResult>();

            try
            {
                if (string.IsNullOrWhiteSpace(userNameOrEmail) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Username/email and password are required.";
                    return response;
                }

                var user = await _userRepository.GetByUserNameOrEmailAsync(userNameOrEmail);

                if (user is null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid credentials.";
                    return response;
                }

                if (!user.IsActive)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Your account is deactivated. Please contact the Support Manager.";
                    return response;
                }

                var ok = _passwordHash.Verify(user.PasswordHash, password);
                if (!ok)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid credentials.";
                    return response;
                }

                response.Data = new AuthResult
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    UserType = user.UserType
                };

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginAsync failed.");

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = null;
            }

            return response;
        }

        public async Task<Response<int>> RegisterClientAsync(RegisterClientModel model)
        {
            var response = new Response<int>();

            try
            {
                if (model == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Model is null.";
                    response.Data = 0;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(model.FullName) ||
                    string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.UserName) ||
                    string.IsNullOrWhiteSpace(model.Password) ||
                    string.IsNullOrWhiteSpace(model.CompanyName))
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Missing required fields.";
                    response.Data = 0;
                    return response;
                }

                var email = model.Email.Trim();
                var userName = model.UserName.Trim();


                var existing =
                    await _userRepository.GetByUserNameOrEmailAsync(userName)
                    ?? await _userRepository.GetByUserNameOrEmailAsync(email);

                if (existing != null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Username or Email already exists.";
                    response.Data = 0;
                    return response;
                }

                var user = new User
                {
                    FullName = model.FullName.Trim(),
                    Email = email,
                    PhoneNumber = model.PhoneNumber?.Trim(),
                    Address = model.Address?.Trim(),
                    DateOfBirth = model.DateOfBirth,

                    UserName = userName,
                    PasswordHash = _passwordHash.Hash(model.Password),
                    IsActive = true,
                    UserType = UserType.ExternalClient,
                    CreatedAt = DateTime.UtcNow,

                    ClientProfile = new ClientProfile
                    {
                        CompanyName = model.CompanyName.Trim(),
                        CompanyAddress = model.CompanyAddress?.Trim(),
                        VatNumber = model.VatNumber?.Trim(),
                        PreferredLanguage = string.IsNullOrWhiteSpace(model.PreferredLanguage)
                            ? "en"
                            : model.PreferredLanguage.Trim()
                    }
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
                response.Data = user.Id;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterClientAsync failed. Email={Email}", model?.Email);
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = 0;
                return response;
            }
        }

        public async Task<Response<ForgotPasswordResultModel?>> ForgotPasswordAsync(ForgotPasswordRequestModel model)
        {
            var res = new Response<ForgotPasswordResultModel?>();

            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.UserNameOrEmail))
                {
                    res.ErrorCode = ErrorCode.GeneralError;
                    res.MsgError = "Username or Email is required.";
                    res.Data = null;
                    return res;
                }

                var user = await _userRepository.GetByUserNameOrEmailAsync(model.UserNameOrEmail);


                if (user == null)
                {
                    res.ErrorCode = ErrorCode.Success;
                    res.MsgError = ErrorCode.Success.ToString();
                    res.Data = null;
                    return res;
                }


                var code = Generate6DigitCode();
                var expires = DateTime.UtcNow.AddMinutes(10);


                user.PasswordResetCodeHash = _passwordHash.Hash(code);
                user.PasswordResetCodeExpiresAt = expires;
                user.PasswordResetAttempts = 0;
                user.UpdatedAt = DateTime.UtcNow;



                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                res.ErrorCode = ErrorCode.Success;
                res.MsgError = ErrorCode.Success.ToString();
                res.Data = new ForgotPasswordResultModel
                {
                    Email = user.Email,
                    Code = code,
                    ExpiresAt = expires
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForgotPasswordAsync failed.");

                res.ErrorCode = ErrorCode.GeneralError;
                res.MsgError = ex.Message;
                res.Data = null;
            }

            return res;
        }

        public async Task<Response<bool>> ResetPasswordAsync(ResetPasswordModel model)
        {
            var res = new Response<bool>();

            try
            {
                if (model == null ||
                    string.IsNullOrWhiteSpace(model.UserNameOrEmail) ||
                    string.IsNullOrWhiteSpace(model.Code) ||
                    string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    res.ErrorCode = ErrorCode.GeneralError;
                    res.MsgError = "Missing data.";
                    res.Data = false;
                    return res;
                }

                var user = await _userRepository.GetByUserNameOrEmailAsync(model.UserNameOrEmail);

                if (user == null)
                {
                    res.ErrorCode = ErrorCode.GeneralError;
                    res.MsgError = "Invalid code or user.";
                    res.Data = false;
                    return res;
                }

                if (user.PasswordResetCodeExpiresAt == null ||
                    user.PasswordResetCodeExpiresAt < DateTime.UtcNow ||
                    string.IsNullOrWhiteSpace(user.PasswordResetCodeHash))
                {
                    res.ErrorCode = ErrorCode.GeneralError;
                    res.MsgError = "Code expired.";
                    res.Data = false;
                    return res;
                }

                if (user.PasswordResetAttempts >= 5)
                {
                    res.ErrorCode = ErrorCode.GeneralError;
                    res.MsgError = "Too many attempts. Request a new code.";
                    res.Data = false;
                    return res;
                }

                var ok = _passwordHash.Verify(user.PasswordResetCodeHash, model.Code);
                if (!ok)
                {
                    user.PasswordResetAttempts += 1;
                    user.UpdatedAt = DateTime.UtcNow;


                    _userRepository.Update(user);
                    await _userRepository.SaveChangesAsync();

                    res.ErrorCode = ErrorCode.GeneralError;
                    res.MsgError = "Invalid code.";
                    res.Data = false;
                    return res;
                }


                user.PasswordHash = _passwordHash.Hash(model.NewPassword);


                user.PasswordResetCodeHash = null;
                user.PasswordResetCodeExpiresAt = null;
                user.PasswordResetAttempts = 0;
                user.UpdatedAt = DateTime.UtcNow;


                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                res.ErrorCode = ErrorCode.Success;
                res.MsgError = ErrorCode.Success.ToString();
                res.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResetPasswordAsync failed.");

                res.ErrorCode = ErrorCode.GeneralError;
                res.MsgError = ex.Message;
                res.Data = false;
            }

            return res;
        }


        private static string Generate6DigitCode()
        {
            var num = RandomNumberGenerator.GetInt32(0, 1000000);
            return num.ToString("D6");
        }
    }
}

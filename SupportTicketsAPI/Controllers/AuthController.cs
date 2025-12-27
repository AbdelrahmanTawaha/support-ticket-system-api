
using BusinessLayer.Models;
using BusinessLayer.Responses;
using BusinessLayer.Services.LogginLogout;
using BusinessLayer.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketsAPI.DTOs;
using SupportTicketsAPI.Services.Auth;
using SupportTicketsAPI.Services.Email;
namespace SupportTicketsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _email;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserManagementService _userManagement;
        private readonly IWebHostEnvironment _env;

        public AuthController(
            IAuthService authService,
            IJwtTokenService jwtTokenService,
            IEmailService email,
            ILogger<AuthController> logger,
         IUserManagementService userManagement,
            IWebHostEnvironment env)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
            _email = email;
            _logger = logger;
            _userManagement = userManagement;
            _env = env;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            var apiResponse = new ApiResponse<LoginResponseDto>();

            try
            {

                if (request == null)
                {
                    apiResponse.ErrorCode = (int)ErrorCode.GeneralError;
                    apiResponse.MsgError = "Request body is required.";
                    return BadRequest(apiResponse);
                }

                if (string.IsNullOrWhiteSpace(request.UserNameOrEmail) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    apiResponse.ErrorCode = (int)ErrorCode.GeneralError;
                    apiResponse.MsgError = "Username/email and password are required.";
                    return BadRequest(apiResponse);
                }


                var result = await _authService.LoginAsync(
                    request.UserNameOrEmail,
                    request.Password);

                if (result == null)
                {
                    _logger.LogWarning("Login business returned null. UserNameOrEmail={UserNameOrEmail}", request.UserNameOrEmail);

                    apiResponse.ErrorCode = (int)ErrorCode.GeneralError;
                    apiResponse.MsgError = "Unexpected null response from business layer.";
                    return StatusCode(StatusCodes.Status500InternalServerError, apiResponse);
                }

                if (result.ErrorCode != ErrorCode.Success || result.Data == null)
                {
                    _logger.LogInformation(
                        "Login failed. UserNameOrEmail={UserNameOrEmail}, ErrorCode={ErrorCode}, Msg={Msg}",
                        request.UserNameOrEmail, result.ErrorCode, result.MsgError);

                    apiResponse.ErrorCode = (int)result.ErrorCode;
                    apiResponse.MsgError = result.MsgError ?? "Login failed.";
                    apiResponse.Data = null;

                    return BadRequest(apiResponse);
                }


                var tokenResult = _jwtTokenService.GenerateToken(result.Data);


                var dto = new LoginResponseDto
                {
                    Token = tokenResult.Token,
                    ExpiresAt = tokenResult.ExpiresAt,
                    FullName = result.Data.FullName,
                    UserName = result.Data.UserName,
                    Role = (result.Data.UserType).ToString()
                };

                apiResponse.ErrorCode = (int)ErrorCode.Success;
                apiResponse.MsgError = ErrorCode.Success.ToString();
                apiResponse.Data = dto;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login exception. UserNameOrEmail={UserNameOrEmail}", request?.UserNameOrEmail);

                apiResponse.ErrorCode = (int)ErrorCode.GeneralError;
                apiResponse.MsgError = "An unexpected error occurred while processing login.";

                apiResponse.MsgError += " | Details: " + ex.Message;

                apiResponse.Data = null;

                return StatusCode(StatusCodes.Status500InternalServerError, apiResponse);
            }
        }

        [HttpPost("register-client")]
        [AllowAnonymous]
        [RequestSizeLimit(10_000_000)]
        public async Task<ActionResult<Response<int>>> RegisterClient([FromForm] RegisterClientFormDto dto)
        {
            var api = new Response<int>();

            try
            {
                if (dto == null)
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Request body is required.";
                    api.Data = 0;
                    return BadRequest(api);
                }


                if (string.IsNullOrWhiteSpace(dto.FullName) ||
                    string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.UserName) ||
                    string.IsNullOrWhiteSpace(dto.Password) ||
                    string.IsNullOrWhiteSpace(dto.CompanyName))
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Missing required fields.";
                    api.Data = 0;
                    return BadRequest(api);
                }

                if (dto.Password.Trim().Length < 6)
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Password must be at least 6 characters.";
                    api.Data = 0;
                    return BadRequest(api);
                }


                var model = new RegisterClientModel
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Address = dto.Address,
                    DateOfBirth = dto.DateOfBirth,

                    UserName = dto.UserName,
                    Password = dto.Password,

                    CompanyName = dto.CompanyName,
                    CompanyAddress = dto.CompanyAddress,
                    VatNumber = dto.VatNumber,
                    PreferredLanguage = dto.PreferredLanguage
                };

                var result = await _authService.RegisterClientAsync(model);

                api.ErrorCode = result.ErrorCode;
                api.MsgError = result.MsgError;
                api.Data = result.Data;

                if (result.ErrorCode != ErrorCode.Success || result.Data <= 0)
                    return BadRequest(api);

                var userId = result.Data;


                if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
                {
                    var save = await SaveUserImageAsync(dto.ProfileImage);

                    if (save.ErrorCode == ErrorCode.Success && !string.IsNullOrWhiteSpace(save.Path))
                    {
                        var setRes = await _userManagement.SetProfileImageAsync(userId, save.Path);

                        if (setRes.ErrorCode != ErrorCode.Success)
                        {
                            api.MsgError = "Registered, but failed to save image url: " + (setRes.MsgError ?? "Unknown error");
                            return Ok(api);
                        }
                    }
                    else
                    {
                        api.MsgError = "Registered, but image upload failed: " + (save.Msg ?? "Unknown error");
                        return Ok(api);
                    }
                }

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterClient exception. Email={Email}", dto?.Email);

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = 0;
                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }



        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<Response<bool>>> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            var api = new Response<bool>();

            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.UserNameOrEmail))
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Username or Email is required.";
                    api.Data = false;
                    return BadRequest(api);
                }

                var model = new ForgotPasswordRequestModel
                {
                    UserNameOrEmail = dto.UserNameOrEmail
                };

                var result = await _authService.ForgotPasswordAsync(model);

                api.ErrorCode = ErrorCode.Success;
                api.MsgError = "If the account exists, a reset code has been sent to the email.";
                api.Data = true;

                if (result.ErrorCode == ErrorCode.Success && result.Data != null)
                {
                    var emailBody = EmailTemplates.ResetCode(result.Data.Code, result.Data.ExpiresAt);

                    await _email.SendAsync(
                        result.Data.Email,
                        "Password Reset Code",
                        emailBody,
                        isHtml: true
                    );
                }

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForgotPassword exception. UserNameOrEmail={UserNameOrEmail}", dto?.UserNameOrEmail);

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = false;
                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<Response<bool>>> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            var api = new Response<bool>();

            try
            {
                if (dto == null ||
                    string.IsNullOrWhiteSpace(dto.UserNameOrEmail) ||
                    string.IsNullOrWhiteSpace(dto.Code) ||
                    string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Missing data.";
                    api.Data = false;
                    return BadRequest(api);
                }

                var model = new ResetPasswordModel
                {
                    UserNameOrEmail = dto.UserNameOrEmail,
                    Code = dto.Code,
                    NewPassword = dto.NewPassword
                };

                var result = await _authService.ResetPasswordAsync(model);

                api.ErrorCode = result.ErrorCode;
                api.MsgError = result.MsgError;
                api.Data = result.Data;

                if (result.ErrorCode != ErrorCode.Success)
                {
                    _logger.LogInformation(
                        "ResetPassword failed. UserNameOrEmail={UserNameOrEmail}, Msg={Msg}",
                        dto.UserNameOrEmail, result.MsgError);

                    return BadRequest(api);
                }

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResetPassword exception. UserNameOrEmail={UserNameOrEmail}", dto?.UserNameOrEmail);

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = false;
                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

        private async Task<SaveFileResult> SaveUserImageAsync(IFormFile file)
        {
            var result = new SaveFileResult();

            try
            {
                if (file == null || file.Length <= 0)
                {
                    result.ErrorCode = ErrorCode.GeneralError;
                    result.Msg = "Empty file.";
                    return result;
                }

                const long maxBytes = 3 * 1024 * 1024;
                if (file.Length > maxBytes)
                {
                    result.ErrorCode = ErrorCode.GeneralError;
                    result.Msg = "Image too large. Max 3MB.";
                    return result;
                }

                var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };

                if (string.IsNullOrWhiteSpace(file.ContentType) || !allowedTypes.Contains(file.ContentType))
                {
                    result.ErrorCode = ErrorCode.GeneralError;
                    result.Msg = "Only JPG/PNG/WEBP are allowed.";
                    return result;
                }

                var ext = Path.GetExtension(file.FileName);
                var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
                if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext))
                {
                    result.ErrorCode = ErrorCode.GeneralError;
                    result.Msg = "Invalid image extension.";
                    return result;
                }

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var folder = Path.Combine(webRoot, "uploads", "users");
                Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(folder, fileName);

                await using (var stream = System.IO.File.Create(fullPath))
                {
                    await file.CopyToAsync(stream);
                }

                result.Path = $"/uploads/users/{fileName}";
                result.ErrorCode = ErrorCode.Success;
                result.Msg = "Saved.";
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveUserImageAsync failed.");
                result.ErrorCode = ErrorCode.GeneralError;
                result.Msg = ex.Message;
                return result;
            }
        }
    }

}


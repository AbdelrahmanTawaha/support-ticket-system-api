
using BusinessLayer.Responses;
using BusinessLayer.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketsAPI.Services;

namespace SupportTicketsAPI.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserManagementService _service;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProfileController> _logger;

        private const string RootFolder = "uploads";
        private const string UsersFolder = "users";

        public ProfileController(
            IUserManagementService service,
            IWebHostEnvironment env,
            ILogger<ProfileController> logger)
        {
            _service = service;
            _env = env;
            _logger = logger;
        }

        [HttpPost("image")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Response<string>>> UploadProfileImage(
      IFormFile file,
     CancellationToken ct)
        {
            var api = new Response<string>();

            try
            {
                var userId = ClaimsHelper.GetUserId(User);
                if (userId == null)
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Unauthorized.";
                    return Unauthorized(api);
                }

                if (file == null || file.Length == 0)
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "File is required.";
                    return BadRequest(api);
                }

                if (file.ContentType == null || !file.ContentType.StartsWith("image/"))
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Invalid content type.";
                    return BadRequest(api);
                }

                var safeOriginal = Path.GetFileName(file.FileName);
                var ext = Path.GetExtension(safeOriginal).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowed.Contains(ext))
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Invalid image type.";
                    return BadRequest(api);
                }

                const long maxSize = 2 * 1024 * 1024;
                if (file.Length > maxSize)
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Image is too large.";
                    return BadRequest(api);
                }

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var userDir = Path.Combine(webRoot, RootFolder, UsersFolder, userId.Value.ToString());
                Directory.CreateDirectory(userDir);

                var stored = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(userDir, stored);

                await using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, ct);
                }

                var relativePath = $"/{RootFolder}/{UsersFolder}/{userId}/{stored}";

                var saveRes = await _service.SetProfileImageAsync(userId.Value, relativePath);

                if (saveRes.ErrorCode != ErrorCode.Success)
                {
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);

                    api.ErrorCode = saveRes.ErrorCode;
                    api.MsgError = saveRes.MsgError;
                    return BadRequest(api);
                }

                api.ErrorCode = ErrorCode.Success;
                api.MsgError = ErrorCode.Success.ToString();
                api.Data = relativePath;

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadProfileImage failed.");

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

    }
}

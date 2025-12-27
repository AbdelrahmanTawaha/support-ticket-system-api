using System.Security.Claims;
using BusinessLayer.Models;
using BusinessLayer.Responses;
using BusinessLayer.Services.UserManagement;
using DataAccessLayer.ConfigurationsSetting.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketsAPI.DTOs;

using SupportTicketsAPI.Services;

namespace SupportTicketsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Manager)]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _service;
        private readonly ILogger<UsersController> _logger;
        private readonly IWebHostEnvironment _env;
        public UsersController(IUserManagementService service, ILogger<UsersController> logger, IWebHostEnvironment env)
        {
            _service = service;
            _logger = logger;
            _env = env;

        }

        [HttpGet("counts-paged")]
        public async Task<ActionResult<ApiPageResponse<List<UserTicketCountDto>>>> GetUsersCountsPaged(
             int pageNumber = 1,
             int pageSize = 10,
             int? userType = null,
             bool? isActive = null,
             string? searchTerm = null)
        {
            var api = new ApiPageResponse<List<UserTicketCountDto>>();

            try
            {
                UserType? typeEnum = null;
                if (userType.HasValue && Enum.IsDefined(typeof(UserType), userType.Value))
                    typeEnum = (UserType)userType.Value;

                var result = await _service.GetEmployeesAndClientsWithTicketsCountPagedAsync(
                    pageNumber, pageSize,
                    typeEnum, isActive, searchTerm);

                if (result == null)
                {
                    _logger.LogWarning("GetEmployeesAndClientsWithTicketsCountPagedAsync returned null.");

                    api.ErrorCode = (int)ErrorCode.GeneralError;
                    api.MsgError = "Unexpected null response from business layer.";
                    api.Data = null;
                    api.TotalCount = 0;

                    return StatusCode(StatusCodes.Status500InternalServerError, api);
                }

                if (result.ErrorCode != ErrorCode.Success || result.Data == null)
                {
                    _logger.LogInformation("GetUsersCountsPaged failed. ErrorCode={ErrorCode}, Msg={Msg}",
                        result.ErrorCode, result.MsgError);

                    api.ErrorCode = (int)result.ErrorCode;
                    api.MsgError = result.MsgError ?? "Failed to load users.";
                    api.Data = null;
                    api.TotalCount = 0;

                    return BadRequest(api);
                }

                api.Data = result.Data.Select(x => new UserTicketCountDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    UserType = x.UserType,
                    IsActive = x.IsActive,
                    TicketsCount = x.TicketsCount,
                    ImageUrl = x.ImageUrl
                }).ToList();

                api.TotalCount = result.TotalCount;
                api.ErrorCode = (int)ErrorCode.Success;
                api.MsgError = ErrorCode.Success.ToString();

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUsersCountsPaged exception.");

                api.ErrorCode = (int)ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;
                api.TotalCount = 0;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

        //  Activate / Deactivate
        [HttpPut("{id:int}/active")]
        public async Task<ActionResult<Response<bool>>> SetActive(
            int id,
            [FromBody] SetUserActiveRequest req)
        {
            try
            {
                var result = await _service.SetUserActiveAsync(id, req.IsActive);

                if (result == null)
                {
                    _logger.LogWarning("SetUserActiveAsync returned null. id={Id}", id);

                    var apiNull = new Response<bool>
                    {
                        ErrorCode = ErrorCode.GeneralError,
                        MsgError = "Unexpected null response from business layer.",
                        Data = false
                    };

                    return StatusCode(StatusCodes.Status500InternalServerError, apiNull);
                }

                var apiResponse = new Response<bool>
                {
                    ErrorCode = result.ErrorCode,
                    MsgError = result.MsgError,
                    Data = result.Data
                };

                if (result.ErrorCode != ErrorCode.Success)
                    return BadRequest(apiResponse);

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetActive exception. id={Id}", id);

                var api = new Response<bool>
                {
                    ErrorCode = ErrorCode.GeneralError,
                    MsgError = ex.Message,
                    Data = false
                };

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

        // Support employees list for Assign dropdown
        [HttpGet("support-employees")]
        public async Task<ActionResult<Response<List<EmployeeOptionDto>>>> GetSupportEmployees()
        {
            try
            {
                var result = await _service.GetSupportEmployeesAsync();

                if (result == null)
                {
                    _logger.LogWarning("GetSupportEmployeesAsync returned null.");

                    var apiNull = new Response<List<EmployeeOptionDto>>
                    {
                        ErrorCode = ErrorCode.GeneralError,
                        MsgError = "Unexpected null response from business layer.",
                        Data = null
                    };

                    return StatusCode(StatusCodes.Status500InternalServerError, apiNull);
                }

                var apiResponse = new Response<List<EmployeeOptionDto>>
                {
                    ErrorCode = result.ErrorCode,
                    MsgError = result.MsgError,
                    Data = result.Data?.Select(x => new EmployeeOptionDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsActive = x.IsActive
                    }).ToList()
                };

                if (result.ErrorCode != ErrorCode.Success)
                    return BadRequest(apiResponse);

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSupportEmployees exception.");

                var api = new Response<List<EmployeeOptionDto>>
                {
                    ErrorCode = ErrorCode.GeneralError,
                    MsgError = ex.Message,
                    Data = null
                };

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Response<UserEditDto>>> GetUserById(int id)
        {
            try
            {
                var result = await _service.GetUserByIdForEditAsync(id);

                if (result == null)
                {
                    _logger.LogWarning("GetUserByIdForEditAsync returned null. id={Id}", id);

                    var apiNull = new Response<UserEditDto>
                    {
                        ErrorCode = ErrorCode.GeneralError,
                        MsgError = "Unexpected null response from business layer.",
                        Data = null
                    };

                    return StatusCode(StatusCodes.Status500InternalServerError, apiNull);
                }

                var api = new Response<UserEditDto>
                {
                    ErrorCode = result.ErrorCode,
                    MsgError = result.MsgError,
                    Data = result.Data == null ? null : new UserEditDto
                    {
                        Id = result.Data.Id,
                        FullName = result.Data.FullName,
                        Email = result.Data.Email,
                        PhoneNumber = result.Data.PhoneNumber,
                        Address = result.Data.Address,
                        DateOfBirth = result.Data.DateOfBirth,

                        UserType = result.Data.UserType,
                        IsActive = result.Data.IsActive
                    }
                };

                if (result.ErrorCode != ErrorCode.Success)
                    return BadRequest(api);

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserById exception. id={Id}", id);

                var api = new Response<UserEditDto>
                {
                    ErrorCode = ErrorCode.GeneralError,
                    MsgError = ex.Message,
                    Data = null
                };

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult<Response<bool>>> UpdateUser(
            int id,
            [FromBody] UpdateUserRequestDto dto)
        {
            try
            {
                var model = new UserEditModel
                {
                    Id = id,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Address = dto.Address,
                    DateOfBirth = dto.DateOfBirth,

                };

                var result = await _service.UpdateUserAsync(id, model);

                if (result == null)
                {
                    _logger.LogWarning("UpdateUserAsync returned null. id={Id}", id);

                    var apiNull = new Response<bool>
                    {
                        ErrorCode = ErrorCode.GeneralError,
                        MsgError = "Unexpected null response from business layer.",
                        Data = false
                    };

                    return StatusCode(StatusCodes.Status500InternalServerError, apiNull);
                }

                var api = new Response<bool>
                {
                    ErrorCode = result.ErrorCode,
                    MsgError = result.MsgError,
                    Data = result.Data
                };

                if (result.ErrorCode != ErrorCode.Success)
                    return BadRequest(api);

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateUser exception. id={Id}", id);

                var api = new Response<bool>
                {
                    ErrorCode = ErrorCode.GeneralError,
                    MsgError = ex.Message,
                    Data = false
                };

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

        [HttpGet("external-clients")]
        public async Task<ActionResult<Response<List<EmployeeOptionDto>>>> GetExternalClients()
        {
            try
            {
                var result = await _service.GetExternalClientsAsync();

                if (result == null)
                {
                    _logger.LogWarning("GetExternalClientsAsync returned null.");

                    var apiNull = new Response<List<EmployeeOptionDto>>
                    {
                        ErrorCode = ErrorCode.GeneralError,
                        MsgError = "Unexpected null response from business layer.",
                        Data = null
                    };

                    return StatusCode(StatusCodes.Status500InternalServerError, apiNull);
                }

                var apiResponse = new Response<List<EmployeeOptionDto>>
                {
                    ErrorCode = result.ErrorCode,
                    MsgError = result.MsgError,
                    Data = result.Data?.Select(x => new EmployeeOptionDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsActive = x.IsActive
                    }).ToList()
                };

                if (result.ErrorCode != ErrorCode.Success)
                    return BadRequest(apiResponse);

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetExternalClients exception.");

                var api = new Response<List<EmployeeOptionDto>>
                {
                    ErrorCode = ErrorCode.GeneralError,
                    MsgError = ex.Message,
                    Data = null
                };

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }

        [HttpPost("support-employees")]
        public async Task<ActionResult<Response<int>>> CreateSupportEmployee(
     [FromBody] CreateSupportEmployeeRequestDto dto)
        {
            try
            {
                var managerIdStr =
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    User.FindFirstValue("id");

                int managerUserId = 0;
                int.TryParse(managerIdStr, out managerUserId);

                var model = new CreateSupportEmployeeModel
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    UserName = dto.UserName,
                    Password = dto.Password,

                    EmployeeCode = dto.EmployeeCode,
                    HireDate = dto.HireDate,
                    Salary = dto.Salary,
                    JobTitle = dto.JobTitle
                };

                var result = await _service.CreateSupportEmployeeAsync(model, managerUserId);

                if (result == null)
                {
                    _logger.LogWarning("CreateSupportEmployeeAsync returned null. managerId={ManagerId}", managerUserId);

                    var apiNull = new Response<int>
                    {
                        ErrorCode = ErrorCode.GeneralError,
                        MsgError = "Unexpected null response from business layer.",
                        Data = 0
                    };

                    return StatusCode(StatusCodes.Status500InternalServerError, apiNull);
                }

                var api = new Response<int>
                {
                    ErrorCode = result.ErrorCode,
                    MsgError = result.MsgError,
                    Data = result.Data
                };

                if (result.ErrorCode != ErrorCode.Success)
                    return BadRequest(api);

                return Ok(api);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateSupportEmployee exception.");

                var api = new Response<int>
                {
                    ErrorCode = ErrorCode.GeneralError,
                    MsgError = ex.Message,
                    Data = 0
                };

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }


        [HttpPost("{id:int}/image")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Response<string>>> UploadUserImage(
            int id,
             IFormFile file,
            CancellationToken ct)
        {
            var api = new Response<string>();

            try
            {
                if (id <= 0)
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "Invalid user id.";
                    return BadRequest(api);
                }

                if (file == null || file.Length == 0)
                {
                    api.ErrorCode = ErrorCode.GeneralError;
                    api.MsgError = "File is required.";
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
                const string RootFolder = "uploads";
                const string UsersFolder = "users";

                var userDir = Path.Combine(webRoot, RootFolder, UsersFolder, id.ToString());
                Directory.CreateDirectory(userDir);

                var stored = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(userDir, stored);

                await using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, ct);
                }

                var relativePath = $"/{RootFolder}/{UsersFolder}/{id}/{stored}";

                var saveRes = await _service.SetProfileImageAsync(id, relativePath);

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
                _logger.LogError(ex, "UploadUserImage failed. id={Id}", id);

                api.ErrorCode = ErrorCode.GeneralError;
                api.MsgError = ex.Message;
                api.Data = null;

                return StatusCode(StatusCodes.Status500InternalServerError, api);
            }
        }


    }
}

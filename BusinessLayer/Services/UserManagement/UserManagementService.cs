using BusinessLayer.Models;
using BusinessLayer.Responses;
using BusinessLayer.Services.PasswordHashService;
using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.ConfigurationsSetting.Enums;
using DataAccessLayer.Repositories.ticket.Interface;
using DataAccessLayer.Repositories.user;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Services.UserManagement
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IPasswordHashService _passwordHash;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            IUserRepository userRepository,
            ITicketRepository ticketRepository,
            IPasswordHashService passwordHashService,
            ILogger<UserManagementService> logger)
        {
            _userRepository = userRepository;
            _ticketRepository = ticketRepository;
            _passwordHash = passwordHashService;
            _logger = logger;
        }

        public async Task<Response<List<UserTicketCountModel>>> GetEmployeesAndClientsWithTicketsCountAsync()
        {
            var response = new Response<List<UserTicketCountModel>>();

            try
            {
                var users = await _userRepository.GetEmployeesAndClientsAsync();

                var ids = users.Select(u => u.Id).ToList();
                var countsDict = await _ticketRepository.GetTicketsCountByUsersAsync(ids);

                response.Data = users.Select(u => new UserTicketCountModel
                {
                    Id = u.Id,
                    Name = u.FullName,
                    UserType = u.UserType,
                    IsActive = u.IsActive,
                    TicketsCount = countsDict.TryGetValue(u.Id, out var c) ? c : 0,
                    ImageUrl = u.ImageUrl
                }).ToList();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetEmployeesAndClientsWithTicketsCountAsync failed.");
                response.Data = null;
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
            }

            return response;
        }

        public async Task<Response<bool>> SetUserActiveAsync(int targetUserId, bool isActive)
        {
            var response = new Response<bool>();

            try
            {
                if (targetUserId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid user id.";
                    response.Data = false;
                    return response;
                }

                var ok = await _userRepository.SetActiveAsync(targetUserId, isActive);

                if (!ok)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "User not found.";
                    response.Data = false;
                    return response;
                }

                response.Data = true;
                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetUserActiveAsync failed. targetUserId={UserId}, isActive={IsActive}",
                    targetUserId, isActive);

                response.Data = false;
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
            }

            return response;
        }

        public async Task<Response<List<UserSimpleModel>>> GetSupportEmployeesAsync()
        {
            var response = new Response<List<UserSimpleModel>>();

            try
            {
                var employees = await _userRepository.GetByUserTypeAsync(UserType.SupportEmployee);

                response.Data = employees.Select(e => new UserSimpleModel
                {
                    Id = e.Id,
                    Name = e.FullName ?? e.UserName ?? "",
                    IsActive = e.IsActive
                }).ToList();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSupportEmployeesAsync failed.");
                response.Data = null;
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
            }

            return response;
        }

        public async Task<PageResponse<List<UserTicketCountModel>>> GetEmployeesAndClientsWithTicketsCountPagedAsync(
            int pageNumber,
            int pageSize,
            UserType? userType,
            bool? isActive,
            string? searchTerm)
        {
            var response = new PageResponse<List<UserTicketCountModel>>();

            try
            {
                var (users, totalCount) = await _userRepository.GetEmployeesAndClientsPagedAsync(
                    pageNumber, pageSize,
                    userType, isActive, searchTerm);

                var ids = users.Select(u => u.Id).ToList();
                var countsDict = ids.Count == 0
                    ? new Dictionary<int, int>()
                    : await _ticketRepository.GetTicketsCountByUsersAsync(ids);

                response.Data = users.Select(u => new UserTicketCountModel
                {
                    Id = u.Id,
                    Name = u.FullName ?? u.UserName ?? "",
                    UserType = u.UserType,
                    IsActive = u.IsActive,
                    TicketsCount = countsDict.TryGetValue(u.Id, out var c) ? c : 0,
                    ImageUrl = u.ImageUrl
                }).ToList();

                response.TotalCount = totalCount;
                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetEmployeesAndClientsWithTicketsCountPagedAsync failed.");
                response.Data = null;
                response.TotalCount = 0;
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
            }

            return response;
        }

        public async Task<Response<bool>> UpdateUserAsync(int userId, UserEditModel model)
        {
            var response = new Response<bool>();

            try
            {
                if (model == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Model is null.";
                    response.Data = false;
                    return response;
                }

                if (userId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid user id.";
                    response.Data = false;
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "User not found.";
                    response.Data = false;
                    return response;
                }

                if (user.UserType != UserType.SupportEmployee &&
                    user.UserType != UserType.ExternalClient)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Only employees and clients can be edited from this screen.";
                    response.Data = false;
                    return response;
                }

                user.FullName = model.FullName?.Trim() ?? user.FullName;
                user.Email = model.Email?.Trim() ?? user.Email;
                user.PhoneNumber = model.PhoneNumber?.Trim() ?? user.PhoneNumber;
                user.Address = model.Address?.Trim();
                user.DateOfBirth = model.DateOfBirth;



                user.UpdatedAt = DateTime.UtcNow;

                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateUserAsync failed. userId={UserId}", userId);
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = false;
            }

            return response;
        }

        public async Task<Response<UserEditModel?>> GetUserByIdForEditAsync(int id)
        {
            var response = new Response<UserEditModel?>();

            try
            {
                if (id <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid user id.";
                    response.Data = null;
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "User not found.";
                    response.Data = null;
                    return response;
                }

                if (user.UserType != UserType.SupportEmployee &&
                    user.UserType != UserType.ExternalClient)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Editing this user type is not allowed.";
                    response.Data = null;
                    return response;
                }

                response.Data = new UserEditModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth,

                    ImageUrl = user.ImageUrl,

                    UserType = user.UserType,
                    IsActive = user.IsActive
                };


                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserByIdForEditAsync failed. id={Id}", id);
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = null;
            }

            return response;
        }

        public async Task<Response<List<UserSimpleModel>>> GetExternalClientsAsync()
        {
            var response = new Response<List<UserSimpleModel>>();

            try
            {
                var clients = await _userRepository.GetByUserTypeAsync(UserType.ExternalClient);

                response.Data = clients.Select(c => new UserSimpleModel
                {
                    Id = c.Id,
                    Name = c.FullName ?? c.UserName ?? "",
                    IsActive = c.IsActive,
                    UserType = c.UserType
                }).ToList();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetExternalClientsAsync failed.");
                response.Data = null;
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
            }

            return response;
        }

        public async Task<Response<int>> CreateSupportEmployeeAsync(CreateSupportEmployeeModel model, int managerUserId)
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

                if (managerUserId <= 0)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid manager id.";
                    response.Data = 0;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(model.FullName) ||
                    string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.Password))
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Missing required fields.";
                    response.Data = 0;
                    return response;
                }

                var email = model.Email.Trim();
                var userName = !string.IsNullOrWhiteSpace(model.UserName)
                    ? model.UserName.Trim()
                    : email.Split('@')[0];

                var existing = await _userRepository.GetByUserNameOrEmailAsync(userName)
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
                    UserType = UserType.SupportEmployee,
                    CreatedAt = DateTime.UtcNow,

                    EmployeeProfile = new EmployeeProfile
                    {
                        EmployeeCode = model.EmployeeCode?.Trim(),
                        HireDate = model.HireDate ?? DateTime.UtcNow,
                        Salary = model.Salary ?? 0,
                        JobTitle = model.JobTitle ?? EmployeeJobTitle.EmployeeSupport,
                        ManagerUserId = managerUserId
                    }
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
                response.Data = user.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateSupportEmployeeAsync failed. managerUserId={ManagerId}", managerUserId);

                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = 0;
            }

            return response;
        }


        public async Task<Response<bool>> SetProfileImageAsync(int userId, string imagePath)
        {
            var response = new Response<bool>();

            try
            {
                if (userId <= 0 || string.IsNullOrWhiteSpace(imagePath))
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "Invalid user id or image path.";
                    response.Data = false;
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.ErrorCode = ErrorCode.GeneralError;
                    response.MsgError = "User not found.";
                    response.Data = false;
                    return response;
                }

                user.ImageUrl = imagePath.Trim();
                user.UpdatedAt = DateTime.UtcNow;


                await _userRepository.SaveChangesAsync();

                response.ErrorCode = ErrorCode.Success;
                response.MsgError = ErrorCode.Success.ToString();
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetProfileImageAsync failed. userId={UserId}", userId);
                response.ErrorCode = ErrorCode.GeneralError;
                response.MsgError = ex.Message;
                response.Data = false;
            }

            return response;
        }

    }
}

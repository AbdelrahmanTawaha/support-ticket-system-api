using BusinessLayer.Models;
using BusinessLayer.Responses;
using DataAccessLayer.ConfigurationsSetting.Enums;

namespace BusinessLayer.Services.UserManagement
{
    public interface IUserManagementService
    {
        Task<Response<List<UserTicketCountModel>>> GetEmployeesAndClientsWithTicketsCountAsync();
        Task<Response<bool>> SetUserActiveAsync(int targetUserId, bool isActive);
        Task<Response<List<UserSimpleModel>>> GetSupportEmployeesAsync();

        Task<PageResponse<List<UserTicketCountModel>>> GetEmployeesAndClientsWithTicketsCountPagedAsync(
            int pageNumber,
            int pageSize,
            UserType? userType,
            bool? isActive,
            string? searchTerm);

        Task<Response<UserEditModel?>> GetUserByIdForEditAsync(int id);
        Task<Response<bool>> UpdateUserAsync(int userId, UserEditModel model);
        Task<Response<List<UserSimpleModel>>> GetExternalClientsAsync();
        Task<Response<int>> CreateSupportEmployeeAsync(CreateSupportEmployeeModel model, int managerUserId);

        Task<Response<bool>> SetProfileImageAsync(int userId, string imagePath);
    }
}

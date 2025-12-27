using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.ConfigurationsSetting.Enums;

namespace DataAccessLayer.Repositories.user
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail);

        Task<List<User>> GetEmployeesAndClientsAsync();

        Task<List<User>> GetByUserTypeAsync(UserType type);

        Task<bool> SetActiveAsync(int userId, bool isActive);
        Task<(List<User> Items, int TotalCount)> GetEmployeesAndClientsPagedAsync(
       int pageNumber,
       int pageSize,
       UserType? userType = null,
       bool? isActive = null,
       string? searchTerm = null);

        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUserNameAsync(string userName);



    }

}

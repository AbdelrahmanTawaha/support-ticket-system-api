using DataAccessLayer.ConfigurationsSetting;
using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.ConfigurationsSetting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer.Repositories.user
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
            : base(context)
        {
            _logger = logger;
        }

        public async Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userNameOrEmail))
                    return null;

                var key = userNameOrEmail.Trim();

                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u =>
                        u.UserName == key || u.Email == key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByUserNameOrEmailAsync failed. key={Key}", userNameOrEmail);
                throw;
            }
        }

        public override async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0) return null;

                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync failed. id={Id}", id);
                throw;
            }
        }

        public async Task<bool> SetActiveAsync(int userId, bool isActive)
        {
            try
            {
                if (userId <= 0) return false;

                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null) return false;

                user.IsActive = isActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetActiveAsync failed. userId={UserId}, isActive={IsActive}", userId, isActive);
                throw;
            }
        }

        public async Task<List<User>> GetEmployeesAndClientsAsync()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Where(u =>
                        u.UserType == UserType.SupportEmployee ||
                        u.UserType == UserType.ExternalClient)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetEmployeesAndClientsAsync failed.");
                throw;
            }
        }

        public async Task<List<User>> GetByUserTypeAsync(UserType type)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Where(u => u.UserType == type)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByUserTypeAsync failed. type={Type}", type);
                throw;
            }
        }

        public async Task<(List<User> Items, int TotalCount)> GetEmployeesAndClientsPagedAsync(
            int pageNumber,
            int pageSize,
            UserType? userType = null,
            bool? isActive = null,
            string? searchTerm = null)
        {
            try
            {
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;

                var query = _context.Users
                    .AsNoTracking()
                    .Where(u =>
                        u.UserType == UserType.SupportEmployee ||
                        u.UserType == UserType.ExternalClient)
                    .AsQueryable();

                if (userType.HasValue)
                    query = query.Where(u => u.UserType == userType.Value);

                if (isActive.HasValue)
                    query = query.Where(u => u.IsActive == isActive.Value);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();

                    query = query.Where(u =>
                        (u.FullName != null && u.FullName.Contains(term)) ||
                        u.UserName.Contains(term) ||
                        u.Email.Contains(term) ||
                        u.Id.ToString().Contains(term));
                }

                query = query.OrderByDescending(u => u.CreatedAt);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetEmployeesAndClientsPagedAsync failed. pageNumber={PageNumber}, pageSize={PageSize}, userType={UserType}, isActive={IsActive}, searchTerm={SearchTerm}",
                    pageNumber, pageSize, userType, isActive, searchTerm);

                throw;
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;

                var key = email.Trim();

                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Email == key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByEmailAsync failed. email={Email}", email);
                throw;
            }
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                    return null;

                var key = userName.Trim();

                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserName == key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByUserNameAsync failed. userName={UserName}", userName);
                throw;
            }
        }
    }
}

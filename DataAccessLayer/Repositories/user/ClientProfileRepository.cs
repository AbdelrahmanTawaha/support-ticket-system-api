using DataAccessLayer.ConfigurationsSetting;
using DataAccessLayer.ConfigurationsSetting.Entity;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer.Repositories.user
{
    public class ClientProfileRepository : IClientProfileRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClientProfileRepository> _logger;

        public ClientProfileRepository(
            AppDbContext context,
            ILogger<ClientProfileRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(ClientProfile profile)
        {
            try
            {
                if (profile == null)
                    throw new ArgumentNullException(nameof(profile));

                await _context.ClientProfiles.AddAsync(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddAsync failed for ClientProfile.");
                throw;
            }
        }

        public Task<int> SaveChangesAsync()
        {
            try
            {
                return _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveChangesAsync failed in ClientProfileRepository.");
                throw;
            }
        }
    }
}

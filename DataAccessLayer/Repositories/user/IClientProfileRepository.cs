using DataAccessLayer.ConfigurationsSetting.Entity;

namespace DataAccessLayer.Repositories.user
{
    public interface IClientProfileRepository
    {
        Task AddAsync(ClientProfile profile);
        Task<int> SaveChangesAsync();
    }
}

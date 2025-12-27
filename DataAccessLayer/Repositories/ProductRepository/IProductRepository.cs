using DataAccessLayer.ConfigurationsSetting.Entity;

namespace DataAccessLayer.Repositories.ProductRepository
{
    public interface IProductRepository
    {
        Task<List<Product>> GetActiveAsync();
    }
}

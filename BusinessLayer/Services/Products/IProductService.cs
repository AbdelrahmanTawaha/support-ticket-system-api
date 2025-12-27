using BusinessLayer.Models;
using BusinessLayer.Responses;
namespace BusinessLayer.Services.Products
{
    public interface IProductService
    {
        Task<Response<List<ProductOptionModel>>> GetActiveAsync();
    }
}

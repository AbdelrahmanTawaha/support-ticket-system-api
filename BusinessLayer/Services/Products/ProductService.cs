using BusinessLayer.Models;
using BusinessLayer.Responses;
using DataAccessLayer.Repositories.ProductRepository;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repo, ILogger<ProductService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Response<List<ProductOptionModel>>> GetActiveAsync()
        {
            var res = new Response<List<ProductOptionModel>>();

            try
            {
                var list = await _repo.GetActiveAsync();

                res.Data = list.Select(p => new ProductOptionModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code
                }).ToList();

                res.ErrorCode = ErrorCode.Success;
                res.MsgError = ErrorCode.Success.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetActiveAsync failed in ProductService.");

                res.Data = null;
                res.ErrorCode = ErrorCode.GeneralError;
                res.MsgError = ex.Message;
            }

            return res;
        }
    }
}

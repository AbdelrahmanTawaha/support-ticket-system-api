using BusinessLayer.Models;
using BusinessLayer.Responses;
using BusinessLayer.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SupportTicketsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService service, ILogger<ProductsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("active")]
        public async Task<ActionResult<Response<List<ProductOptionModel>>>> GetActive()
        {
            try
            {
                var result = await _service.GetActiveAsync();

                if (result.ErrorCode != ErrorCode.Success)
                {
                    _logger.LogInformation("GetActive failed: {Msg}", result.MsgError);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetActive error");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response<List<ProductOptionModel>>
                    {
                        ErrorCode = ErrorCode.GeneralError,
                        MsgError = ex.Message,
                        Data = null
                    });
            }
        }
    }
}

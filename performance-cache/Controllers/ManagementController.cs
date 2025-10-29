using Domain;
using Microsoft.AspNetCore.Mvc;
using performance_cache.Service;
using StackExchange.Redis;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace performance_cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagementController : ControllerBase
    {
        private const string cacheKey = "product-cache";
        private string redisConnection;
        private readonly IProductService _productService;
        private readonly IStockMovementService _stockMovementService;
        private readonly IReportService _reportService;
        private readonly ILogger<ManagementController> _logger;

        public ManagementController(IProductService productService,
                                    IStockMovementService stockMovementService,
                                    IReportService reportService,
                                    IConfiguration configuration,
                                    ILogger<ManagementController> logger)
        {
            _productService = productService;
            _stockMovementService = stockMovementService;
            _reportService = reportService;
            _logger = logger;

            redisConnection = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
        }

        #region Product Management

        // POST: api/management/product
        [HttpPost("product")]
        public IActionResult AddProduct([FromBody] Product product)
        {
            try
            {
                if (product == null)
                {
                    _logger.LogWarning("Tentativa de adicionar produto com dados nulos");
                    return BadRequest(new { message = "Produto é obrigatório", timestamp = DateTime.UtcNow });
                }

                if (!_productService.ValidateProduct(product))
                {
                    _logger.LogWarning("Produto inválido: {ProductName}", product.Name);
                    return BadRequest(new { message = "Produto inválido. Verifique os campos obrigatórios", timestamp = DateTime.UtcNow });
                }

                _productService.AddProduct(product);
                _logger.LogInformation("Produto adicionado: {ProductName}", product.Name);
                return CreatedAtAction(nameof(GetProductById), new { id = product.SKUCode }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao adicionar produto");
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "Erro interno do servidor ao adicionar produto", timestamp = DateTime.UtcNow });
            }
        }

        // GET: api/management/product/{id}
        [HttpGet("product/{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _productService.GetAllProducts().FirstOrDefault(p => p.SKUCode == id);
            if (product == null)
            {
                return NotFound(new { message = "Produto não encontrado", timestamp = DateTime.UtcNow });
            }

            return Ok(product);
        }

        #endregion

        #region Stock Movement

        // POST: api/management/stockmovement/{skuCode}
        [HttpPost("stockmovement/{skuCode}")]
        public IActionResult RegisterStockMovement(int skuCode, [FromBody] StockMovement stockMovement)
        {
            try
            {
                // Buscar o produto pelo SKUCode
                var product = _productService.GetAllProducts().FirstOrDefault(p => p.SKUCode == skuCode);
                if (product == null)
                {
                    return NotFound(new { message = "Produto não encontrado", timestamp = DateTime.UtcNow });
                }

                // Registrar a movimentação de estoque
                _stockMovementService.RegisterMovement(product, stockMovement);
                _logger.LogInformation("Movimentação de estoque registrada para produto SKU: {SKUCode}", skuCode);
                return Ok(new { message = "Movimentação registrada com sucesso", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar movimentação de estoque");
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "Erro interno ao registrar movimentação", timestamp = DateTime.UtcNow });
            }
        }

        #endregion

        #region Report Management

        // GET: api/management/report/stockvalue
        [HttpGet("report/stockvalue")]
        public async Task<IActionResult> GetTotalStockValue()
        {
            try
            {
                var totalValue = await _reportService.GetTotalStockValueAsync();
                _logger.LogInformation("Valor total do estoque calculado: {TotalValue}", totalValue);
                return Ok(new { totalStockValue = totalValue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular o valor total do estoque");
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "Erro interno ao calcular o valor total do estoque", timestamp = DateTime.UtcNow });
            }
        }

        // GET: api/management/report/expiringsoon
        [HttpGet("report/expiringsoon")]
        public async Task<IActionResult> GetProductsExpiringSoon()
        {
            try
            {
                var products = await _reportService.GetProductsExpiringSoonAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar produtos vencendo em breve");
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "Erro interno ao listar produtos vencendo", timestamp = DateTime.UtcNow });
            }
        }

        // GET: api/management/report/lowstock
        [HttpGet("report/lowstock")]
        public async Task<IActionResult> GetProductsBelowMinimumStock()
        {
            try
            {
                var products = await _reportService.GetProductsBelowMinimumStockAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar produtos abaixo do estoque mínimo");
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "Erro interno ao listar produtos com estoque baixo", timestamp = DateTime.UtcNow });
            }
        }

        #endregion

        #region Cache Management

        private async Task InvalidateCache()
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect(redisConnection);
                IDatabase dbRedis = redis.GetDatabase();
                await dbRedis.KeyDeleteAsync(cacheKey);
                _logger.LogInformation("Cache invalidado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao invalidar cache Redis, mas operação continuará");
            }
        }

        #endregion
    }
}

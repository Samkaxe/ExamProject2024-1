using AutoMapper;
using Core.Dtos;
using Core.Entites;
using InventoryService.Extintions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InventoryService.Controllers;

    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly MongoService _mongoService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IMapper mapper, MongoService mongoService, ILogger<ProductController> logger)
        {
            _mapper = mapper;
            _mongoService = mongoService;
            _logger = logger;
        }

        [HttpGet("CheckConnection")]
        public IActionResult CheckConnection()
        {
            try
            {
                if (_mongoService.IsConnectionEstablished())
                {
                    return Ok("Connection to MongoDB is established.");
                }
                else
                {
                    return Problem("Unable to establish connection to MongoDB.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking the MongoDB connection.");
                return Problem("An error occurred while checking the MongoDB connection.");
            }
        }

        [HttpGet("brands")]
        public IActionResult GetAllBrands()
        {
            try
            {
                var brands = _mongoService.GetAllBrands();
                var brandDtos = _mapper.Map<List<BrandDto>>(brands);
                return Ok(brandDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving brands.");
                return Problem("An error occurred while retrieving brands.");
            }
        }

        [HttpPost("brands")]
        public IActionResult CreateBrand([FromBody] BrandDto brandDto)
        {
            try
            {
                var brand = _mapper.Map<Brand>(brandDto);
                _mongoService.InsertBrand(brand);
                var brandDtos = _mapper.Map<BrandDto>(brand);
                return CreatedAtAction(nameof(GetAllBrands), new { id = brandDtos.Id }, brandDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a brand.");
                return Problem("An error occurred while creating a brand.");
            }
        }

        [HttpPost("products")]
        public IActionResult CreateProduct([FromBody] CreateProductDto productDto)
        {
            try
            {
                var product = _mapper.Map<Product>(productDto);
                _mongoService.InsertProduct(product);
                var productDtos = _mapper.Map<ProductDto>(product);
                return CreatedAtAction(nameof(GetAllProducts), new { id = productDtos.Id }, productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a product.");
                return Problem("An error occurred while creating a product.");
            }
        }

        [HttpGet("products")]
        public ActionResult<List<ProductDto>> GetAllProducts()
        {
            try
            {
                var products = _mongoService.GetAllProducts();
                var productDtos = new List<ProductDto>();

                foreach (var product in products)
                {
                    var brand = _mongoService.GetBrandById(product.BrandId);
                    var category = _mongoService.GetCategoryById(product.CategoryId);

                    var productDto = _mapper.Map<ProductDto>(product);
                    productDto.Brand = _mapper.Map<BrandDto>(brand);
                    productDto.Category = _mapper.Map<CategoryDto>(category);

                    productDtos.Add(productDto);
                }

                return productDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving products.");
                return Problem("An error occurred while retrieving products.");
            }
        }

        [HttpGet("categories")]
        public IActionResult GetAllCategories()
        {
            try
            {
                var categories = _mongoService.GetAllCategories();
                var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
                return Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving categories.");
                return Problem("An error occurred while retrieving categories.");
            }
        }

        [HttpPost("categories")]
        public IActionResult CreateCategory([FromBody] CategoryDto categoryDto)
        {
            try
            {
                var category = _mapper.Map<Category>(categoryDto);
                _mongoService.InsertCategory(category);
                var categoryDtos = _mapper.Map<CategoryDto>(category);
                return CreatedAtAction(nameof(GetAllCategories), new { id = categoryDtos.Id }, categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a category.");
                return Problem("An error occurred while creating a category.");
            }
        }
    }


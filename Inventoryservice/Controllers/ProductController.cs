using AutoMapper;
using Core.Dtos;
using Core.Entites;
using InventoryService.Extintions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace InventoryService.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly MongoService _mongoService;

    public ProductController(IMapper mapper, MongoService mongoService)
    {
        _mapper = mapper;
        _mongoService = mongoService;
    }
    
    [HttpGet("CheckConnection")]
    public IActionResult CheckConnection()
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
    
    [HttpGet("brands")]
    public IActionResult GetAllBrands()
    {
        var brands = _mongoService.Brands.Find(_ => true).ToList();
        var brandDtos = _mapper.Map<List<BrandDto>>(brands);
        return Ok(brandDtos);
    }
    
    [HttpPost("brands")]
    public IActionResult CreateBrand([FromBody] BrandDto brandDto)
    {
        var brand = _mapper.Map<Brand>(brandDto);
        _mongoService.Brands.InsertOne(brand);
        var brandDtos = _mapper.Map<BrandDto>(brand);
        return CreatedAtAction(nameof(GetAllBrands), new { id = brandDtos.Id }, brandDto);
    }

    [HttpPost("products")]
    public IActionResult CreateProduct([FromBody] CreateProductDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);
        _mongoService.Products.InsertOne(product);
        var productDtos = _mapper.Map<ProductDto>(product);
        return CreatedAtAction(nameof(GetAllProducts), new { id = productDtos.Id }, productDto);
    }

    [HttpGet("products")]
    public ActionResult<List<ProductDto>> GetAllProducts()
    {
        var products = _mongoService.Products.Find(product => true).ToList();
        var productDtos = new List<ProductDto>();

        foreach (var product in products)
        {
            var brand = _mongoService.Brands.Find(b => b.Id == product.BrandId).FirstOrDefault();
            var category = _mongoService.Categories.Find(c => c.Id == product.CategoryId).FirstOrDefault();

            var productDto = _mapper.Map<ProductDto>(product);
            productDto.Brand = _mapper.Map<BrandDto>(brand);
            productDto.Category = _mapper.Map<CategoryDto>(category);

            productDtos.Add(productDto);
        }

        return productDtos;
    }
    
    [HttpGet("categories")]
    public IActionResult GetAllCategories()
    {
        var categories = _mongoService.Categories.Find(_ => true).ToList();
        var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
        return Ok(categoryDtos);
    }

    [HttpPost("categories")]
    public IActionResult CreateCategory([FromBody] CategoryDto categoryDto)
    {
        var category = _mapper.Map<Category>(categoryDto);
        _mongoService.Categories.InsertOne(category);
        var categoryDtos = _mapper.Map<CategoryDto>(category);
        return CreatedAtAction(nameof(GetAllCategories), new { id = categoryDtos.Id }, categoryDto);
    }
    
}
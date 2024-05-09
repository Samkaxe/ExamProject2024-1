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

    [HttpPost]
    public IActionResult CreateProduct([FromBody] CreateProductDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);
        _mongoService.Products.InsertOne(product);
        var productDtos = _mapper.Map<ProductDto>(product);
        return CreatedAtAction(nameof(GetAllProducts), new { id = productDtos.Id }, productDto);
    }

    [HttpGet]
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
}
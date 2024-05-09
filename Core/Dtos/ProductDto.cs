namespace Core.Dtos;

public class ProductDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public BrandDto Brand { get; set; }
    public CategoryDto Category { get; set; }
}
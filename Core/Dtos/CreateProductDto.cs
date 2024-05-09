namespace Core.Dtos;

public class CreateProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string BrandId { get; set; }
    public string CategoryId { get; set; }
}
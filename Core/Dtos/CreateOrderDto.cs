using System.ComponentModel.DataAnnotations;

namespace Core.Dtos;

public class CreateOrderDto
{
    [Required]
    public string UserId { get; set; }

    [Required]
    public List<OrderItemDto> Items { get; set; }
}
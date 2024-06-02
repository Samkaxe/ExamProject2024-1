using Microsoft.AspNetCore.Mvc;

namespace RabbitMQService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RabbitMQController : ControllerBase
{
    private readonly RabbitMQService _rabbitMQService;

    public RabbitMQController(RabbitMQService rabbitMQService)
    {
        _rabbitMQService = rabbitMQService;
    }
    
    // testing 
    [HttpPost("send-to-checkout")]
    public IActionResult SendToCheckout([FromBody] string message)
    {
        _rabbitMQService.SendMessage("checkout_queue", message);
        return Ok("Message sent to Checkout service.");
    }

    [HttpGet("check-inventory-connection")]
    public async Task<IActionResult> CheckInventoryConnection()
    {
        var result = await _rabbitMQService.CheckInventoryConnectionAsync();
        // Send the result to the checkout queue
        _rabbitMQService.SendMessage("checkout_queue", result);
        return Ok($"Connection check result sent to queue: {result}");
    }
}
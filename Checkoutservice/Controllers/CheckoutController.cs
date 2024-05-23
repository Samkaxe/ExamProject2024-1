using Checkoutservice.Extintions;
using Microsoft.AspNetCore.Mvc;

namespace Checkoutservice.Controllers;

[ApiController]
[Route("[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly RedisCacheService _redisCacheService;

    public CheckoutController(RedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    [HttpPost("start")]
    public IActionResult StartCheckoutSession(string userId, string orderId)
    {
        _redisCacheService.SetCheckoutSession(userId, orderId);
        return Ok($"Checkout session started for User: {userId} with Order: {orderId}");
    }

    [HttpGet("session")]
    public IActionResult GetCheckoutSession(string userId)
    {
        var orderId = _redisCacheService.GetCheckoutOrderId(userId);
        if (orderId != null)
        {
            return Ok($"User: {userId} has an active order: {orderId}");
        }
        return NotFound("No active checkout session found for this user.");
    }
}

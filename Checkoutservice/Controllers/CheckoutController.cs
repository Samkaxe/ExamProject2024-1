using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Checkoutservice.Controllers;

[ApiController]
[Route("[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly RedisCacheService _redisCacheService;
    private static readonly ActivitySource ActivitySource = new ActivitySource("CheckoutService");

    public CheckoutController(RedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    [HttpPost("start")]
    public IActionResult StartCheckoutSession(string userId, string orderId)
    {
        using var activity = ActivitySource.StartActivity("StartCheckoutSession");
        _redisCacheService.SetCheckoutSession(userId, orderId);
        return Ok($"Checkout session started for User: {userId} with Order: {orderId}");
    }

    [HttpGet("session")]
    public IActionResult GetCheckoutSession(string userId)
    {
        using var activity = ActivitySource.StartActivity("GetCheckoutSession");
        var orderId = _redisCacheService.GetCheckoutOrderId(userId);
        if (orderId != null)
        {
            return Ok($"User: {userId} has an active order: {orderId}");
        }
        return NotFound("No active checkout session found for this user.");
    }
}
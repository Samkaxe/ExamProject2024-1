using Core.Dtos;
using InventoryService.Extintions;
using Microsoft.AspNetCore.Mvc;


namespace InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MongoService _mongoService;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(MongoService mongoService, JwtTokenService jwtTokenService)
    {
        _mongoService = mongoService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userDto)
    {
        if (await _mongoService.RegisterUser(userDto))
            return Ok(new { message = "User registered successfully" });

        return BadRequest(new { message = "User already exists" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userDto)
    {
        var user = await _mongoService.AuthenticateUser(userDto);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var token = _jwtTokenService.GenerateToken(user);
        return Ok(new { token });
    }
}

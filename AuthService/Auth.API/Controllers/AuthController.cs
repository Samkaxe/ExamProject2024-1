using Auth.API.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.OperationResults;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController: ControllerBase
{

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<OperationResult<string>>> Register([FromBody] RegisterModel model)
    {
        var result = await _authService.Register(model);
        return Ok(result);
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<OperationResult<string>>> Login([FromBody] LoginModel model)
    {
        return await _authService.Login(model);
    }
}
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Models.Auth;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController: BaseController
{
    private readonly IUserService _userService;
    private readonly IJwtUtils _jwtUtils;

    public AuthController(IUserService userService, IJwtUtils jwtUtils)
    {
        _userService = userService;
        _jwtUtils = jwtUtils;
    }


    [HttpPost("login")]
    public async Task<LoginResponse> Login([FromBody] LoginRequest req)
    {
        LoginResponse loginResult = await _userService.Login(req);
        return loginResult;
    }

}

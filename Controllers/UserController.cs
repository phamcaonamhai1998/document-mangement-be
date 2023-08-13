using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("create")]
    public async Task<CreateUserResponse> Create([FromBody] CreateUserRequest req)
    {
        CreateUserResponse createResult  = await _userService.Create(req);
        return createResult;
    }
}

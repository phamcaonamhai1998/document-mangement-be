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

    [HttpGet]
    public async Task<List<UserDto>> GetAll()
    {
        List<UserDto> users = await _userService.GetAll();
        return users;
    }

    [HttpGet("{id}")]
    public async Task<UserDto> GetById(string id)
    {
        UserDto getResult = await _userService.GetById(id);
        return getResult;
    }

    [HttpPost()]
    public async Task<CreateUserResponse> Create([FromBody] CreateUserRequest req)
    {
        CreateUserResponse createResult  = await _userService.Create(req);
        return createResult;
    }


    [HttpDelete]
    public async Task<bool> Delete(string id)
    {
        bool deleteResult = await _userService.Delete(id);
        return deleteResult;
    }
}

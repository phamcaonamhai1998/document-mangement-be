using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Authorization;
using WebApi.Common.Constants;
using WebApi.Models;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService, IJwtUtils jwtUtils)
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

    [HttpPost]
    public async Task<CreateUserResponse> Create([FromBody] CreateUserRequest req)
    {
        CreateUserResponse createResult = await _userService.Create(req);
        return createResult;
    }


    [HttpPut("{id}")]
    public async Task<bool> Update(string id, [FromBody] UpdateUserRequest req)
    {
        bool updateResult = await _userService.Update(id, req);
        return updateResult;
    }

    [HttpDelete]
    [AuthorizeAttribute("User:Delete")]
    public async Task<bool> Delete(string id)
    {
        bool deleteResult = await _userService.Delete(id);
        return deleteResult;
    }
}

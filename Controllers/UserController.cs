
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Common.Constants;
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
    [AuthorizeAttribute("User:List")]
    public async Task<List<UserDto>> GetAll()
    {
        switch (Claims.Role.Id.ToString())
        {
            case RoleConstants.ADMIN_ROLE_ID:
                return await _userService.GetAll();

            case RoleConstants.ORG_OWNER_ID:
                return await _userService.GetAll();

            case RoleConstants.DEP_OWNER_ID:
                return await _userService.GetAll();

            default:
                return new List<UserDto>();
        }

    }

    [HttpGet("can-assigns")]
    [AuthorizeAttribute("User:List")]
    public async Task<List<UserDto>> GetUsersCanAssigns()
    {
        return await _userService.GetUsersCanAssign(Claims);

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
    [AuthorizeAttribute("User:Update")]
    public async Task<bool> Update(string id, [FromBody] UpdateUserRequest req)
    {
        bool updateResult = await _userService.Update(id, req);
        return updateResult;
    }

    [HttpDelete("{id}")]
    [AuthorizeAttribute("User:Delete")]
    public async Task<bool> Delete(string id)
    {
        bool deleteResult = await _userService.Delete(id);
        return deleteResult;
    }
}

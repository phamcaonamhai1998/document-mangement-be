
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Authorization;
using WebApi.Common.Constants;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Auth;
using WebApi.Models.DigitalSignature;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    DataContext _dbContext;

    public UserController(IUserService userService, IJwtUtils jwtUtils, DataContext dbContext)
    {
        _userService = userService;
        _dbContext = dbContext;
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
                return await _userService.GetOrgUsers(Claims);

            case RoleConstants.DEP_OWNER_ID:
                return await _userService.GetDepUsers(Claims);

            default:
                return await _userService.GetUsers(Claims);
        }

    }

    [HttpGet("can-assigns")]
    [AuthorizeAttribute("User:Assign")]
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

    [HttpPut("password")]
    [AuthorizeAttribute("User:Update")]
    public async Task<bool> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        bool updateResult = await _userService.ChangePassword(req, Claims);
        return updateResult;
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

    [HttpPost("cert/upload/{userId}")]
    [Authorize]
    [RequestFormLimits(MultipartBoundaryLengthLimit = 104857600)]
    public async Task<string> UploadCert([FromForm] IFormFile file, string userId)
    {
        return await _userService.UploadCert(file, userId);
    }

    [HttpPost("cert")]
    [Authorize]
    public async Task<bool> CreateCert(CreateDigitalSignature req)
    {
        return await _userService.CreateCert(req, Claims);
    }


    [HttpGet("certs")]
    [Authorize]
    public async Task<List<DigitalSignDto>> GetUserCerts()
    {
        return await _userService.GetUserCerts(Claims);
    }


    [HttpPut("certs-default/{id}")]
    [Authorize]
    public async Task<bool> SetCertDefault(string id)
    {
        return await _userService.SetCertDefault(id, Claims);
    }
}

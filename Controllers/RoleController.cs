using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Models.Accounts;
using WebApi.Models.Role;
using WebApi.Models.Roles;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RoleController : BaseController
{

    private readonly IRoleService _service;
    public RoleController(IRoleService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize]
    public Task<string> Create([FromBody] CreateRoleRequest req)
    {
        return _service.Create(req, Claims);
    }

    [Authorize]
    [HttpGet]
    public async Task<List<RoleDto>> Get()
    {
        return null;
    }

    [Authorize]
    [HttpPut]
    public async Task<bool> Update()
    {
        return true;
    }

    [Authorize]
    [HttpDelete]
    public async Task<bool> Delete()
    {
        return true;
    }

}

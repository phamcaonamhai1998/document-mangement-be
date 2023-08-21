
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Models.Accounts;
using WebApi.Models.Procedures;
using WebApi.Models.Role;
using WebApi.Models.Roles;
using WebApi.Models.Users;
using WebApi.Services;
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
    [AuthorizeAttribute("Role:Create")]
    public Task<string> Create([FromBody] CreateRoleRequest req)
    {
        return _service.Create(req, Claims);
    }

    [AuthorizeAttribute("Role:List")]
    [HttpGet]
    public async Task<List<RoleDto>> Get()
    {
        return null;
    }

    [AuthorizeAttribute("Role:List")]
    [HttpGet("all")]
    public async Task<List<RoleDto>> GetAll()
    {

        List<RoleDto> roles = await _service.GetAll(Claims) ;
        return roles;
    }

    [AuthorizeAttribute("Role:Update")]
    [HttpPut]
    public async Task<bool> Update()
    {
        return true;
    }

    [AuthorizeAttribute("Role:Delete")]
    [HttpDelete]
    public async Task<bool> Delete()
    {
        return true;
    }

}

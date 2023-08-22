
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
    public async Task<List<RoleDto>> GetAll()
    {

        List<RoleDto> roles = await _service.GetAll(Claims);
        return roles;
    }

    [AuthorizeAttribute("Role:List")]
    [HttpGet("{id}")]
    public async Task<RoleDetailDto> Get(string id)
    {
        return await _service.Get(id, Claims);
    }

    [AuthorizeAttribute("Role:List")]
    [HttpGet("all")]
    public async Task<List<RoleDto>> GetList()
    {

        List<RoleDto> roles = await _service.GetAll(Claims);
        return roles;
    }

    [AuthorizeAttribute("Role:Update")]
    [HttpPut("{id}")]
    public async Task<bool> Update([FromBody] UpdateRoleRequest request, string id)
    {
        return await _service.Update(id, request, Claims);
    }

    [AuthorizeAttribute("Role:Delete")]
    [HttpDelete("{id}")]
    public async Task<bool> Delete(string id)
    {
        return await _service.Delete(id, Claims);

    }

}

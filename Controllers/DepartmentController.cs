using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Constants;
using WebApi.Models.Departments;
using WebApi.Models.Documents;
using WebApi.Services;
using WebApi.Services.Interfaces;
using WebApi.Authorization;
namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DepartmentController : BaseController
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }


    [HttpGet("org/{orgId}")]
    [AuthorizeAttribute("Department:List")]
    public async Task<List<DepartmentDto>> GetOrgDeps(string orgId)
    {
        return await _departmentService.GetDepsByOrgId(orgId);
    }

    [HttpGet("create-owner/{orgId}")]
    [Authorize]
    public async Task<List<DepartmentDto>> GetAvailableDepsToCreateOwner(string orgId)
    {
        List<DepartmentDto> deps = await _departmentService.GetAvailableDepsToCreateOwner(Claims, orgId);
        return deps;
    }

    [HttpGet]
    public async Task<List<DepartmentDto>> GetAll()
    {
        List<DepartmentDto> result;
        switch (Claims.Role.Id.ToString())
        {
            case RoleConstants.ADMIN_ROLE_ID:
                result = await _departmentService.GetAll(Claims);
                return result;
            case RoleConstants.ORG_OWNER_ID:
                result = await _departmentService.GetOrgDeps(Claims);
                return result;
            default:
                return null;
        }
    }

    [HttpGet("{id}")]
    public async Task<DepartmentDto> GetById(string id)
    {
        DepartmentDto getResult = await _departmentService.GetById(id, Claims);
        return getResult;
    }

    [HttpPost]
    public async Task<CreateDepartmentResponse> Create([FromBody] CreateDepartmentRequest req)
    {
        CreateDepartmentResponse createResult = await _departmentService.Create(req, Claims);
        return createResult;
    }


    [HttpPut("{id}")]
    public async Task<bool> Update(string id, [FromBody] UpdateDepartmentRequest req)
    {
        bool updateResult = await _departmentService.Update(id, req, Claims);
        return updateResult;
    }

    [HttpDelete("{id}")]
    public async Task<bool> Delete(string id)
    {
        bool deleteResult = await _departmentService.Delete(id, Claims);
        return deleteResult;
    }
}

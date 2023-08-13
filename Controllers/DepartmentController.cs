using Microsoft.AspNetCore.Mvc;
using WebApi.Models.Departments;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DepartmentController : Controller
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<List<DepartmentDto>> GetAll()
    {
        List<DepartmentDto> deps = await _departmentService.GetAll();
        return deps;
    }

    [HttpGet("{id}")]
    public async Task<DepartmentDto> GetById(string id)
    {
        DepartmentDto getResult = await _departmentService.GetById(id);
        return getResult;
    }

    [HttpPost]
    public async Task<CreateDepartmentResponse> Create([FromBody] CreateDepartmentRequest req)
    {
        CreateDepartmentResponse createResult  = await _departmentService.Create(req);
        return createResult;
    }


    [HttpPut("{id}")]
    public async Task<bool> Update(string id, [FromBody] UpdateDepartmentRequest req)
    {
        bool updateResult = await _departmentService.Update(id, req);
        return updateResult;
    }

    [HttpDelete]
    public async Task<bool> Delete(string id)
    {
        bool deleteResult = await _departmentService.Delete(id);
        return deleteResult;
    }
}

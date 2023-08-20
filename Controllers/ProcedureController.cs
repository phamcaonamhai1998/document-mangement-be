using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Common.Constants;
using WebApi.Models.Documents;
using WebApi.Models.Procedures;
using WebApi.Services;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ProcedureController : BaseController
{
    private readonly IProcedureService _procedureService;

    public ProcedureController(IProcedureService procedureService)
    {
        _procedureService = procedureService;
    }

    [HttpGet]
    [AuthorizeAttribute("Organization:List")]
    public async Task<List<ProcedureDto>> GetAll()
    {

        List<ProcedureDto> result;
        switch (Claims.Role.Id.ToString())
        {
            case RoleConstants.ADMIN_ROLE_ID:
                result = await _procedureService.GetAll(Claims);
                return result;
            case RoleConstants.ORG_OWNER_ID:
                result = await _procedureService.GetOrgProcedures(Claims);
                return result;
            case RoleConstants.DEP_OWNER_ID:
                result = await _procedureService.GetDepartmentProcedures(Claims);
                return result;
            default:
                return null;
        }
    }

    [HttpGet("{id}")]
    public async Task<ProcedureDto> GetById(string id)
    {
        ProcedureDto getResult = await _procedureService.GetById(id);
        return getResult;
    }

    [HttpPost]
    public async Task<CreateProcedureResponse> Create([FromBody] CreateProcedureRequest req)
    {
        CreateProcedureResponse createResult  = await _procedureService.Create(req, Claims);
        return createResult;
    }


    [HttpPut("{id}")]
    public async Task<bool> Update(string id, [FromBody] UpdateProcedureRequest req)
    {
        bool updateResult = await _procedureService.Update(id, req, Claims);
        return updateResult;
    }

    [HttpDelete]
    public async Task<bool> Delete(string id)
    {
        bool deleteResult = await _procedureService.Delete(id);
        return deleteResult;
    }
}

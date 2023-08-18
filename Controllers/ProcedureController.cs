using Microsoft.AspNetCore.Mvc;
using WebApi.Models.Procedures;
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
    public async Task<List<ProcedureDto>> GetAll()
    {
        List<ProcedureDto> procedures = await _procedureService.GetAll();
        return procedures;
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

using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Models.Organizations;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrganizationController : BaseController
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<List<OrganizationDto>> GetAll()
    {
        List<OrganizationDto> orgs = await _organizationService.GetAll();
        return orgs;
    }

    [HttpGet("create-owner")]
    [Authorize]
    public async Task<List<OrganizationDto>> GetAvailableOrgToCreateOwner()
    {
        List<OrganizationDto> orgs = await _organizationService.GetAvailableOrgToCreateOwner(Claims);
        return orgs;
    }

    [HttpGet("{id}")]
    public async Task<OrganizationDto> GetById(string id)
    {
        OrganizationDto getResult = await _organizationService.GetById(id);
        return getResult;
    }

    [HttpPost]
    public async Task<CreateOrganizationResponse> Create([FromBody] CreateOrganizationRequest req)
    {
        CreateOrganizationResponse createResult  = await _organizationService.Create(req);
        return createResult;
    }

    [HttpPut("{id}")]
    public async Task<bool> Update(string id, [FromBody] UpdateOrganizationRequest req)
    {
        bool updateResult = await _organizationService.Update(id, req);
        return updateResult;
    }

    [HttpDelete("{id}")]
    public async Task<bool> Delete(string id)
    {
        bool deleteResult = await _organizationService.Delete(id);
        return deleteResult;
    }
}

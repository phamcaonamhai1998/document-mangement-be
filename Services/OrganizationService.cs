using AutoMapper;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Common.Constants;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Organizations;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class OrganizationService : IOrganizationService
{
	private readonly DataContext _dbContext;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;

    public OrganizationService(DataContext dbContext, IJwtUtils jwtUtils, IMapper mapper, IOptions<AppSettings> appSettings)
    {
        _dbContext = dbContext;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }
    public Task<CreateOrganizationResponse> Create(CreateOrganizationRequest payload)
    {
		var createOrg = _mapper.Map<Organization>(payload);
		createOrg.Id = Guid.NewGuid();
        _dbContext.Organizations.Add(createOrg);
        _dbContext.SaveChanges();

        return Task.FromResult(new CreateOrganizationResponse(createOrg.Id));
    }

    public Task<bool> Delete(string id)
    {
        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }

        var org = _dbContext.Organizations.SingleOrDefault(a => a.Id == Guid.Parse(id));

        if (org == null) throw new Exception("organization_is_not_found");

        _dbContext.Organizations.Remove(org);
        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }

    public Task<List<OrganizationDto>> GetAll()
    {
        var orgs = _dbContext.Organizations.Where((org)=>org.Id != Guid.Parse(SystemOrg.SystemOrgId)).ToList();
        var orgDtos = _mapper.Map<List<OrganizationDto>>(orgs);
        return Task.FromResult(orgDtos);
    }

    public Task<OrganizationDto> GetById(string id)
    {
         if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }
        var org = _dbContext.Organizations.SingleOrDefault(a => a.Id == Guid.Parse(id));
        OrganizationDto orgDto = _mapper.Map<OrganizationDto>(org);

        return Task.FromResult(orgDto);
    }

    public Task<bool> Update(string id, UpdateOrganizationRequest payload)
    {
        if(payload == null)
        {
            throw new Exception("payload_is_empty");
        }

        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }

        var org = _dbContext.Organizations.SingleOrDefault(a => a.Id == Guid.Parse(id));

        org.Name = payload.Name;
        org.Email = payload.Email;
        org.Phone = payload.Phone;

        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }
}
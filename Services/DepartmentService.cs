using AutoMapper;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Departments;
using WebApi.Models.Organizations;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class DepartmentService : IDepartmentService
{
    private readonly DataContext _dbContext;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;
    private readonly OrganizationService _orgService;

    public DepartmentService(DataContext dbContext, IJwtUtils jwtUtils, IMapper mapper, IOptions<AppSettings> appSettings)
    {
        _dbContext = dbContext;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }

    public Task<CreateDepartmentResponse> Create(CreateDepartmentRequest payload, UserClaims claim)
    {
        var createDep = _mapper.Map<Department>(payload);
        createDep.Id = Guid.NewGuid();
        createDep.CreatedBy = claim.Id;
        _dbContext.Departments.Add(createDep);
        _dbContext.SaveChanges();

        return Task.FromResult(new CreateDepartmentResponse(createDep.Id));
    }

    public Task<bool> Delete(string id, UserClaims claim)
    {
        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }

        var dep = _dbContext.Departments.SingleOrDefault(a => a.Id == Guid.Parse(id));

        if (dep == null) throw new Exception("department_is_not_found");

        _dbContext.Departments.Remove(dep);
        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }

    public Task<List<DepartmentDto>> GetAll(UserClaims claim)
    {
        var deps = _dbContext.Departments.ToList();
        List<DepartmentDto> depDtos = new List<DepartmentDto>();
        deps.ForEach((dep) =>
        {
            var depDto = _mapper.Map<DepartmentDto>(dep);
            depDtos.Add(depDto);
        });
        return Task.FromResult(depDtos);
    }

    public Task<List<DepartmentDto>> GetOrgDeps(UserClaims claim)
    {
        var deps = _dbContext.Departments.Where(dep => dep.Organization.Id == claim.Organization.Id).ToList();
        List<DepartmentDto> depDtos = new List<DepartmentDto>();
        deps.ForEach((dep) =>
        {
            var depDto = _mapper.Map<DepartmentDto>(dep);
            depDtos.Add(depDto);
        });
        return Task.FromResult(depDtos);
    }

    public Task<DepartmentDto> GetById(string id, UserClaims claim)
    {
        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }
        var dep = _dbContext.Departments.SingleOrDefault(a => a.Id == Guid.Parse(id));
        DepartmentDto depDto = _mapper.Map<DepartmentDto>(dep);

        OrganizationDto orgDto = _mapper.Map<OrganizationDto>(dep.Organization);

        depDto.Organization = orgDto;

        return Task.FromResult(depDto);
    }

    public Task<bool> Update(string id, UpdateDepartmentRequest payload, UserClaims claim)
    {
        if (payload == null)
        {
            throw new Exception("payload_is_empty");
        }

        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }

        var dep = _dbContext.Departments.SingleOrDefault(a => a.Id == Guid.Parse(id));

        if (dep == null)
        {
            throw new Exception("dep_is_not_found");
        }

        dep.Name = payload.Name;
        dep.Email = payload.Email;
        dep.Phone = payload.Phone;
        dep.UpdatedBy = claim.Id;

        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }
}
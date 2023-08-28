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
    private readonly StorageHelper _storageHelper;


    public DepartmentService(DataContext dbContext, IJwtUtils jwtUtils, IMapper mapper, IOptions<AppSettings> appSettings, StorageHelper storageHelper)
    {
        _dbContext = dbContext;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
        _storageHelper = storageHelper;

    }

    public async Task<CreateDepartmentResponse> Create(CreateDepartmentRequest payload, UserClaims claim)
    {
        var createDep = _mapper.Map<Department>(payload);
        createDep.Id = Guid.NewGuid();
        createDep.CreatedBy = claim.Id;

        var orgId = !string.IsNullOrEmpty(payload.OrgId) && !string.IsNullOrWhiteSpace(payload.OrgId) ? Guid.Parse(payload.OrgId) : claim.Organization.Id;
        var org = _dbContext.Organizations.SingleOrDefault(org => org.Id == orgId);
        createDep.Organization = org;
        var depDriveFolderId = await _storageHelper.CreateDepFolder(createDep.Name, org.OrgDriveFolderId);
        createDep.DepartmentDriveFolderId = depDriveFolderId;
        _dbContext.Departments.Add(createDep);
        _dbContext.SaveChanges();

        return new CreateDepartmentResponse(createDep.Id);
    }

    public async Task<bool> Delete(string id, UserClaims claim)
    {
        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }

        var dep = _dbContext.Departments.SingleOrDefault(a => a.Id == Guid.Parse(id));

        if (dep.DepartmentDriveFolderId != null)
        {
            var depDriveFolderId = await _storageHelper.DeleteFolder(dep.DepartmentDriveFolderId);
        }

        if (dep == null) throw new Exception("department_is_not_found");

        _dbContext.Departments.Remove(dep);
        _dbContext.SaveChanges();
        return true;
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

    public async Task<bool> Update(string id, UpdateDepartmentRequest payload, UserClaims claim)
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

        await _storageHelper.UpdateFolderName(dep.DepartmentDriveFolderId, payload.Name);
        _dbContext.SaveChanges();
        return true;
    }

    public async Task<List<DepartmentDto>> GetDepsByOrgId(string orgId)
    {
        var deps = _dbContext.Departments.Where(d => d.Organization.Id == Guid.Parse(orgId)).ToList();
        return _mapper.Map<List<DepartmentDto>>(deps);
    }
}
using AutoMapper;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Departments;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class DepartmentService : IDepartmentService
{
    private readonly DataContext _dbContext;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;

    public DepartmentService(DataContext dbContext, IJwtUtils jwtUtils, IMapper mapper, IOptions<AppSettings> appSettings)
    {
        _dbContext = dbContext;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }

    public Task<CreateDepartmentResponse> Create(CreateDepartmentRequest payload)
    {
        var createDep = _mapper.Map<Department>(payload);
		createDep.Id = Guid.NewGuid();
        _dbContext.Departments.Add(createDep);
        _dbContext.SaveChanges();

        return Task.FromResult(new CreateDepartmentResponse(createDep.Id));
    }

    public Task<bool> Delete(string id)
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

    public Task<List<DepartmentDto>> GetAll()
    {
        var orgs = _dbContext.Departments.ToList();
        var depDtos = _mapper.Map<List<DepartmentDto>>(orgs);
        return Task.FromResult(depDtos);
    }

    public Task<DepartmentDto> GetById(string id)
    {
          if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }
        var dep = _dbContext.Departments.SingleOrDefault(a => a.Id == Guid.Parse(id));
        DepartmentDto depDto = _mapper.Map<DepartmentDto>(dep);

        return Task.FromResult(depDto);
    }

    public Task<bool> Update(string id, UpdateDepartmentRequest payload)
    {
        if(payload == null)
        {
            throw new Exception("payload_is_empty");
        }

        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }

        var dep = _dbContext.Departments.SingleOrDefault(a => a.Id == Guid.Parse(id));

        dep.Name = payload.Name;
        dep.Email = payload.Email;
        dep.Phone = payload.Phone;

        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }
}
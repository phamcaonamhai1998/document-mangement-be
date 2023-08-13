using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;
using BCrypt.Net;

namespace WebApi.Services;

public class UserService : IUserService
{
    private readonly DataContext _dbContext;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;

    public UserService(DataContext dbContext, IJwtUtils jwtUtils, IMapper mapper, IOptions<AppSettings> appSettings)
    {
        _dbContext = dbContext;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }

    public Task<CreateUserResponse> Create(CreateUserRequest payload)
    {
        var account = _dbContext.Accounts.SingleOrDefault(x => x.Email == payload.Email);
        if (account != null) throw new Exception("user_email_is_existed");
        var createAccount = _mapper.Map<Account>(payload);

        if (!String.IsNullOrEmpty(payload.Password))
        {
            createAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword(payload.Password);
        }
        else
        {
            throw new Exception("password_is_empty");
        }

        //check role
        if (String.IsNullOrEmpty(payload.RoleId) || String.IsNullOrWhiteSpace(payload.RoleId))
        {
            throw new Exception("role_is_empty");
        }
        else
        {
            var roleId = Guid.Parse(payload.RoleId);
            var role = _dbContext.Roles.SingleOrDefault(r => r.Id == roleId);
            if (role == null) throw new Exception("invalid_role");
        }

        //check org
        if (String.IsNullOrEmpty(payload.OrgId) || String.IsNullOrWhiteSpace(payload.OrgId))
        {
            throw new Exception("org_is_empty");
        }
        else
        {
            var orgId = Guid.Parse(payload.OrgId);
            var org = _dbContext.Organizations.SingleOrDefault(o => o.Id == orgId);
            if (org == null) throw new Exception("invalid_org");
        }

        //check dep
        if (payload.DepartmentId.Count() > 0)
        {
            var depId = Guid.Parse(payload.DepartmentId);
            var dep = _dbContext.Departments.SingleOrDefault(d => d.Id == depId);
            if (dep == null) throw new Exception("invalid_department");
        }

        createAccount.Id = Guid.NewGuid();
        _dbContext.Accounts.Add(createAccount);
        _dbContext.SaveChanges();

        return Task.FromResult(new CreateUserResponse(createAccount.Id));
    }

    public Task<bool> Delete(string id)
    {
        try
        {
            if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
            {

            }

            return Task.FromResult(true);

        } catch(Exception ex) {
            return Task.FromResult(false);
        }
    }
}

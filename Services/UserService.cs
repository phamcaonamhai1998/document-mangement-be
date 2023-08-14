using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;
using BCrypt.Net;
using WebApi.Models.Role;
using WebApi.Models.Auth;

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

        ////check role
        //if (String.IsNullOrEmpty(payload.RoleId) || String.IsNullOrWhiteSpace(payload.RoleId))
        //{
        //    throw new Exception("role_is_empty");
        //}
        //else
        //{
        //    var roleId = Guid.Parse(payload.RoleId);
        //    var role = _dbContext.Roles.SingleOrDefault(r => r.Id == roleId);
        //    if (role == null) throw new Exception("invalid_role");
        //}

        ////check org
        //if (String.IsNullOrEmpty(payload.OrgId) || String.IsNullOrWhiteSpace(payload.OrgId))
        //{
        //    throw new Exception("org_is_empty");
        //}
        //else
        //{
        //    var orgId = Guid.Parse(payload.OrgId);
        //    var org = _dbContext.Organizations.SingleOrDefault(o => o.Id == orgId);
        //    if (org == null) throw new Exception("invalid_org");
        //}

        ////check dep
        //if (payload.DepartmentId.Count() > 0)
        //{
        //    var depId = Guid.Parse(payload.DepartmentId);
        //    var dep = _dbContext.Departments.SingleOrDefault(d => d.Id == depId);
        //    if (dep == null) throw new Exception("invalid_department");
        //}

        createAccount.Id = Guid.NewGuid();
        _dbContext.Accounts.Add(createAccount);
        _dbContext.SaveChanges();

        return Task.FromResult(new CreateUserResponse(createAccount.Id));
    }

    public Task<bool> Delete(string id)
    {
        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }

        var user = _dbContext.Accounts.SingleOrDefault(a => a.Id == Guid.Parse(id));

        if (user == null) throw new Exception("user_is_not_found");

        _dbContext.Accounts.Remove(user);
        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }

    public Task<List<UserDto>> GetAll()
    {
        var users = _dbContext.Accounts.ToList();
        List<UserDto> userDtos = new List<UserDto>();
        users.ForEach(u =>
        {
            var userDto = _mapper.Map<UserDto>(u);
            userDtos.Add(userDto);
        });
        return Task.FromResult(userDtos);

    }

    public Task<UserDto> GetById(string id)
    {
        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }
        var user = _dbContext.Accounts.Include(a => a.Role).SingleOrDefault(a => a.Id == Guid.Parse(id));
        UserDto userDto = _mapper.Map<UserDto>(user);
        RoleDto roleDto = _mapper.Map<RoleDto>(user.Role);

        userDto.Role = roleDto;

        return Task.FromResult(userDto);

    }

    public Task<bool> Update(string id, UpdateUserRequest payload)
    {
        if (payload == null)
        {
            throw new Exception("payload_is_empty");
        }

        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }

        var user = _dbContext.Accounts.SingleOrDefault(a => a.Id == Guid.Parse(id));

        user.IsActive = payload.IsActive;
        user.FirstName = payload.FirstName;
        user.LastName = payload.LastName;
        user.Phone = payload.Phone;

        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }

    public Task<LoginResponse> Login(LoginRequest payload)
    {
        Account user = _dbContext.Accounts
            .Include(a => a.Department)
            .Include(a => a.Role)
            .SingleOrDefault(a => a.Email == payload.Email);

        if (user == null)
        {
            throw new Exception("user_is_not_found");
        }

        bool isValidPwd = BCrypt.Net.BCrypt.Verify(payload.Password, user.PasswordHash);
        if (!isValidPwd)
        {
            throw new Exception("password_is_incorrect");
        }

        RoleDto roleDto = _mapper.Map<RoleDto>(user.Role);

        // get org
        Organization org = _dbContext.Organizations.SingleOrDefault(o => o.Id == Guid.Parse(user.OrgId));

        // get permissions
        List<string> rights = new List<string>();

        if (roleDto != null)
        {

            List<RolePermission> rps = _dbContext.RolePermissions.Where(rp => rp.RoleId == user.Role.Id).ToList();

            rps.ForEach(rp =>
            {
                rights.Add($"{rp.Name}:{rp.Code}");
            });
        }

        UserClaims claims = new UserClaims(user.Id, user.FirstName, user.LastName, roleDto, user.Department, org, rights);
        string token = _jwtUtils.GenerateJwtToken(claims);

        return Task.FromResult(new LoginResponse(token));
    }
}

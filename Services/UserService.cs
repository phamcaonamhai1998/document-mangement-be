﻿using AutoMapper;
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
using WebApi.Common.Constants;
using WebApi.Models.Procedures;
using WebApi.Models.Organizations;
using System.Security.Claims;
using Google.Apis.Drive.v3.Data;
using WebApi.Models.DigitalSignature;
using System.Data;

namespace WebApi.Services;

public class UserService : IUserService
{
    private readonly DataContext _dbContext;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    IWebHostEnvironment _hostingEnvironment;
    StorageHelper _storageHelper;
    private readonly AppSettings _appSettings;

    public UserService(DataContext dbContext, IJwtUtils jwtUtils, IMapper mapper, IOptions<AppSettings> appSettings, IWebHostEnvironment hostingEnvironment, StorageHelper storageHelper)
    {
        _dbContext = dbContext;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
        _hostingEnvironment = hostingEnvironment;
        _storageHelper = storageHelper;
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
            createAccount.Role = role;
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
            createAccount.OrgId = org.Id.ToString();
            if (org == null) throw new Exception("invalid_org");
        }

        //check dep
        if (!string.IsNullOrEmpty(payload.DepartmentId) && payload.DepartmentId.Count() > 0)
        {
            var depId = Guid.Parse(payload.DepartmentId);
            var dep = _dbContext.Departments.SingleOrDefault(d => d.Id == depId);
            if (dep == null) throw new Exception("invalid_department");
            createAccount.Department = dep;
        }

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
        var users = _dbContext.Accounts.Include(a => a.Role).Include(a => a.Department).Where((user) => user.Id != Guid.Parse(SystemOrg.AdminId)).ToList();
        List<UserDto> userDtos = new List<UserDto>();
        users.ForEach(u =>
        {
            var userDto = _mapper.Map<UserDto>(u);
            // if (userDto.Org != null) {
            //     userDto.Org = new Organization(userDto.Org.Id, userDto.Org?.Name);
            // }
            // if(userDto.Department != null) {
            //     userDto.Department = new Department(userDto.Department.Id, userDto.Department?.Name);
            // }
            userDtos.Add(userDto);
        });
        return Task.FromResult(userDtos);

    }

    public Task<List<UserDto>> GetList()
    {
        var users = _dbContext.Accounts.Where((user) => user.Id != Guid.Parse(SystemOrg.AdminId)).ToList();
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
            .SingleOrDefault(a => a.Email == payload.Email && a.IsActive == true);

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

        user.Department = user.Department != null ? new Department(user.Department.Id, user.Department.Name) : null;
        org = new Organization(org.Id, org.Name, org.Phone, org.Email, org.OrgDriveFolderId, org.WebsiteAddress);

        UserClaims claims = new UserClaims(user.Id, user.FirstName, user.LastName, roleDto, user.Department, org, rights, user.Email);
        string token = _jwtUtils.GenerateJwtToken(claims);

        return Task.FromResult(new LoginResponse(token));
    }

    public async Task<List<UserDto>> GetOrgUsers(UserClaims claims)
    {
        try
        {
            var users = _dbContext.Accounts.Include(a => a.Role).Where((user) => user.OrgId == claims.Organization.Id.ToString()).ToList();
            List<UserDto> userDtos = new List<UserDto>();
            users.ForEach(u =>
            {
                var userDto = _mapper.Map<UserDto>(u);
                userDtos.Add(userDto);
            });
            userDtos = userDtos.Where(u => u.Id != claims.Id).ToList();
            return userDtos;

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public async Task<List<UserDto>> GetDepUsers(UserClaims claims)
    {
        try
        {
            var users = _dbContext.Accounts.Include(a => a.Department).Include(a => a.Role).Where((user) => user.Department != null && user.Department.Id == claims.Department.Id).ToList();
            List<UserDto> userDtos = new List<UserDto>();
            users.ForEach(u =>
            {
                var userDto = _mapper.Map<UserDto>(u);
                // userDto.Org = new Organization();
                // userDto.Department = new Department();
                userDtos.Add(userDto);
            });

            userDtos = userDtos.Where(u => u.Id != claims.Id).ToList();

            return userDtos;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<List<UserDto>> GetUsers(UserClaims claims)
    {
        try
        {
            var result = new List<UserDto>();
            if (claims.Rights.Any(r => r == $"{PermissionGroupCode.User}:{PermissionCode.List}"))
            {

                if (claims.Department != null && !string.IsNullOrWhiteSpace(claims.Department.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Department.Id.ToString()))
                {
                    var users = _dbContext.Accounts.Include(a => a.Role).Include(a => a.Department).Where(a => a.Department.Id == claims.Department.Id).ToList();
                    result = _mapper.Map<List<UserDto>>(users);
                }
                else if (claims.Organization != null && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && claims.Organization.Id.ToString() != SystemOrg.SystemOrgId)
                {
                    var users = _dbContext.Accounts.Include(a => a.Role).Include(a => a.Department).Where(a => a.OrgId != null && a.OrgId == claims.Organization.Id.ToString()).ToList();
                    result = _mapper.Map<List<UserDto>>(users);
                }
            }

            result = result.Where(u => u.Id != claims.Id).ToList();
            return result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<List<UserDto>> GetUsersCanAssign(UserClaims claims)
    {
        try
        {
            //get all role can approve
            var rolePermissions = _dbContext.RolePermissions.Where(rp => rp.Name == PermissionGroupCode.Document && rp.Code == PermissionCode.Approve).ToList();

            var roleIds = rolePermissions.Select(rp => rp.RoleId).ToList();

            var cmd = _dbContext.Accounts.Where((user) => user.Id != Guid.Parse(SystemOrg.AdminId)).Include(a => a.Role);

            var users = new List<Account>();


            if (claims.Rights.Count() > 0 && claims.Rights.Any(r => r == $"{PermissionGroupCode.User}:{PermissionCode.Assign}"))
            {
                if (claims.Department != null && !string.IsNullOrWhiteSpace(claims.Department.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Department.Id.ToString()))
                {
                    users = cmd.Include(a => a.Department).Where((user) => user.Department != null && user.Department.Id == claims.Department.Id).ToList();
                }
                else if (claims.Organization != null && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && claims.Organization.Id.ToString() != SystemOrg.SystemOrgId)
                {
                    users = cmd.Where((user) => user.OrgId == claims.Organization.Id.ToString()).ToList();
                }
            }

            var canAssignUsers = users.Where(u => roleIds.Any(id => id == u.Role.Id)).ToList();

            var userDtos = _mapper.Map<List<UserDto>>(canAssignUsers);

            return userDtos;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<bool> ChangePassword(ChangePasswordRequest payload, UserClaims claims)
    {
        Account user = _dbContext.Accounts.SingleOrDefault(a => a.Id.ToString() == claims.Id.ToString());

        if (user == null)
        {
            throw new Exception("user_is_not_found");
        }

        bool isValidPwd = BCrypt.Net.BCrypt.Verify(payload.Password, user.PasswordHash);
        if (!isValidPwd)
        {
            throw new Exception("password_is_incorrect");
        }

        var newPwd = BCrypt.Net.BCrypt.HashPassword(payload.NewPassword);
        user.PasswordHash = newPwd;

        _dbContext.Accounts.Update(user);
        _dbContext.SaveChanges();
        return true;
    }

    public async Task<string> UploadCert(IFormFile file, string userId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId))
        {
            throw new Exception("user_id_is_empty");
        }
        string path = Path.Combine("~/", "DigitalSigns");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string fileName = Path.GetFileName(file.Name);
        string fileId = "";
        Account user = _dbContext.Accounts.Include(a => a.Department).SingleOrDefault(a => a.Id == Guid.Parse(userId));
        Organization org = new Organization();
        Department dep = new Department();
        if (user.OrgId != null)
        {
            org = _dbContext.Organizations.SingleOrDefault(a => a.Id == Guid.Parse(user.OrgId));
        }
        if (user.Department != null)
        {
            dep = _dbContext.Departments.SingleOrDefault(a => a.Id == user.Department.Id);
        }

        //create cert folder for user if not exist
        if (user.CertFolderId == null)
        {
            var _certFolderId = await _storageHelper.CreateUserCertFolder(user.Id.ToString(), $"{user.FirstName} {user.LastName}");
            user.CertFolderId = _certFolderId;
            _dbContext.Update(user);
            _dbContext.SaveChanges();
        }

        using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
        {
            file.CopyTo(stream);
            string fileMime = MimeMapping.MimeUtility.GetMimeMapping(file.FileName);
            string driveFolderId = null;

            if (org != null && org.OrgDriveFolderId != null && org.OrgDriveFolderId.Count() > 0) driveFolderId = org.OrgDriveFolderId;
            if (dep != null && dep.DepartmentDriveFolderId != null && dep.DepartmentDriveFolderId.Count() > 0) driveFolderId = dep.DepartmentDriveFolderId;
            fileId = await _storageHelper.UploadCert(stream, file.Name, user.CertFolderId);
        }
        return fileId;
    }

    public Task<List<DigitalSignDto>> GetUserCerts(UserClaims claims)
    {
        try
        {
            var certs = _dbContext.DigitalSignature.Where(ds => ds.User.Id == claims.Id).ToList();
            //List<DigitalSignDto> certDtos = new List<DigitalSignDto>();

            //certs.ForEach((cert) =>
            //{
            //    var certDto = _mapper.Map<DigitalSignDto>(cert);
            //    certDtos.Add(certDto);
            //});
            //return Task.FromResult(certDtos);
            return Task.FromResult(_mapper.Map<List<DigitalSignDto>>(certs));
        }
        catch (Exception err)
        {
            throw err;
        }
    }
    public Task<bool> SetCertDefault(string id, UserClaims claims)
    {
        try
        {

            var cert = _dbContext.DigitalSignature.Where(ds => ds.User.Id == claims.Id && ds.Id == Guid.Parse(id)).SingleOrDefault();
            if (cert == null)
            {
                throw new Exception("cert_is_not_found");
            }

            _dbContext.DigitalSignature.Where(ds => ds.User.Id == claims.Id).ExecuteUpdate(setter => setter.SetProperty(ds => ds.IsDefault, false));

            cert.IsDefault = true;
            _dbContext.DigitalSignature.Update(cert);
            _dbContext.SaveChanges();
            return Task.FromResult(true);
        }
        catch (Exception err)
        {
            throw err;
        }
    }

    public async Task<bool> CreateCert(CreateDigitalSignature payload, UserClaims claims)
    {
        Account user = _dbContext.Accounts.Include(a => a.Department).SingleOrDefault(a => a.Id == claims.Id);
        Organization org = new Organization();
        Department dep = new Department();
        if (user.OrgId != null)
        {
            org = _dbContext.Organizations.SingleOrDefault(a => a.Id == Guid.Parse(user.OrgId));
        }
        if (user.Department != null)
        {
            dep = _dbContext.Departments.SingleOrDefault(a => a.Id == user.Department.Id);
        }

        _dbContext.DigitalSignature.Where(ds => ds.User.Id == user.Id).ExecuteUpdate(setter => setter.SetProperty(ds => ds.IsDefault, false));

        //save file to digital signature table
        var certFile = await _storageHelper.GetFile(payload.FileId);
        var newDigitalSign = new DigitalSignature();
        newDigitalSign.User = user;
        newDigitalSign.Path = certFile.WebContentLink;
        newDigitalSign.FileId = payload.FileId;
        newDigitalSign.Name = $"{user.Id} - {user.FirstName} {user.LastName} - {DateTime.Now.ToLocalTime()}";
        newDigitalSign.IsDefault = true;
        newDigitalSign.HashPassword = BCrypt.Net.BCrypt.HashPassword(payload.Password);
        _dbContext.DigitalSignature.Add(newDigitalSign);
        _dbContext.SaveChanges();
        return true;
    }
}

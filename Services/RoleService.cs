using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Data;
using WebApi.Authorization;
using WebApi.Common.Constants;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Permissions;
using WebApi.Models.Procedures;
using WebApi.Models.Role;
using WebApi.Models.Roles;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class RoleService : IRoleService
    {
        private readonly DataContext _dbContext;
        private readonly IMapper _mapper;

        public RoleService(DataContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public Task<string> Create(CreateRoleRequest request, UserClaims _claims)
        {
            // create role
            Role role = new Role();
            role.Id = Guid.NewGuid();
            role.Name = request.Name;
            role.CreatedBy = _claims.Id;
            role.OrgId = _claims.Organization.Id.ToString();
            role.DepartmentId = _claims.Department != null ? _claims.Department.Id.ToString() : null;
            _dbContext.Roles.Add(role);
            _dbContext.SaveChanges();

            // add role permissions
            request.Permissions.ForEach(permission =>
            {
                RolePermission rp = new RolePermission(role.Id, permission.GroupCode, permission.Code);
                _dbContext.RolePermissions.Add(rp);
            });
            _dbContext.SaveChanges();
            return Task.FromResult(role.Id.ToString());
        }

        public async Task<RoleDetailDto> Get(string id, UserClaims claims)
        {
            Role role = await getRoleById(Guid.Parse(id));

            if (claims.Organization.Id.ToString() != role.OrgId)
            {
                throw new Exception("role_not_exist_in_org");
            }

            if (claims.Department != null && claims.Department.Id.ToString() != role.DepartmentId)
            {
                throw new Exception("role_not_exist_in_department");
            }

            List<RolePermission> rolePermissions = _dbContext.RolePermissions.Where(rp => rp.RoleId == role.Id).ToList();


            RoleDetailDto result = new RoleDetailDto();
            result.Permissions = _mapper.Map<List<RolePermissionDto>>(rolePermissions);
            result.Id = role.Id.ToString();
            result.Name = role.Name;

            return result;

        }

        public Task<List<RoleDto>> GetAll(UserClaims claims)
        {
            var command = _dbContext.Roles.Where(role => role.Id != Guid.Parse(SysRole.Admin));

            switch (claims.Role.Id.ToString())
            {
                case RoleConstants.ORG_OWNER_ID:
                    command = command.Where(r => r.OrgId == claims.Organization.Id.ToString() || r.Id == Guid.Parse(RoleConstants.DEP_OWNER_ID));
                    break;
                case RoleConstants.DEP_OWNER_ID:
                    command = command.Where(r => r.OrgId == claims.Organization.Id.ToString());
                    break;
                default:
                    break;
            }

            var roles = command.ToList();
            List<RoleDto> roleDtos = new List<RoleDto>();

            roles.ForEach((role) =>
            {
                var roleDto = _mapper.Map<RoleDto>(role);
                roleDtos.Add(roleDto);
            });

            return Task.FromResult(roleDtos);
        }

        public Task<List<RoleDto>> GetList(UserClaims claims)
        {
            try
            {
                var command = _dbContext.Roles.Where(role => role.Id != Guid.Parse(SysRole.Admin)
                                        && role.Id != Guid.Parse(RoleConstants.ORG_OWNER_ID)
                                        && role.Id != Guid.Parse(RoleConstants.DEP_OWNER_ID));
                var roles = command.ToList();
                List<RoleDto> roleDtos = new List<RoleDto>();

                roles.ForEach((role) =>
                {
                    var roleDto = _mapper.Map<RoleDto>(role);
                    roleDtos.Add(roleDto);
                });
                return Task.FromResult(roleDtos);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<bool> Update(string id, UpdateRoleRequest request, UserClaims claims)
        {
            var role = _dbContext.Roles.SingleOrDefault(r => r.Id == Guid.Parse(id));
            if (role == null)
            {
                throw new Exception("role_is_not_found");
            }

            role.Name = request.Name;
            role.UpdatedBy = claims.Id;

            _dbContext.Roles.Update(role);
            _dbContext.SaveChanges();

            var oldPermissions = _dbContext.RolePermissions.Where(rp => rp.RoleId == Guid.Parse(id)).ToList();
            if (oldPermissions.Count() > 0)
            {
                oldPermissions.ForEach(op => _dbContext.RolePermissions.Remove(op));
            }

            // add new role permissions
            request.Permissions.ForEach(permission =>
            {
                RolePermission rp = new RolePermission(role.Id, permission.GroupCode, permission.Code);
                _dbContext.RolePermissions.Add(rp);
            });

            _dbContext.SaveChanges();
            return true;
        }
        public async Task<bool> Delete(string id, UserClaims claims)
        {

            var role = _dbContext.Roles.SingleOrDefault(r => r.Id == Guid.Parse(id));
            if (role == null)
            {
                throw new Exception("role_is_not_found");
            }

            var oldPermissions = _dbContext.RolePermissions.Where(rp => rp.Id == Guid.Parse(id)).ToList();
            if (oldPermissions.Count() > 0)
            {
                oldPermissions.ForEach(op => _dbContext.RolePermissions.Remove(op));
            }

            _dbContext.Roles.Remove(role);
            _dbContext.SaveChanges();
            return true;
        }


        private async Task<Role> getRoleById(Guid id)
        {

            Role role = _dbContext.Roles.Where(r => r.Id == id).SingleOrDefault();
            return role;

        }

        public async Task<List<RoleDto>> GetAvailableRoles(UserClaims claims)
        {
            var result = new List<RoleDto>();
            try
            {
                if (claims.Rights.Any(r => r == $"{PermissionGroupCode.Role}:{PermissionCode.Assign}"))
                {
                    if (claims.Department != null && !string.IsNullOrWhiteSpace(claims.Department.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Department.Id.ToString()))
                    {
                        var roles = _dbContext.Roles.Where(r => r.DepartmentId == claims.Department.Id.ToString()).ToList();
                        result = _mapper.Map<List<RoleDto>>(roles);
                        return result;
                    }

                    if (claims.Organization != null && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && claims.Organization.Id.ToString() != SystemOrg.SystemOrgId)
                    {
                        var roles = _dbContext.Roles.Where(r => r.OrgId == claims.Organization.Id.ToString()).ToList();
                        result = _mapper.Map<List<RoleDto>>(roles);
                        return result;
                    }

                    if (claims.Organization != null && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && claims.Organization.Id.ToString() == SystemOrg.SystemOrgId)
                    {

                        // user is admin
                        var roles = _dbContext.Roles.Where(r => r.Id != Guid.Parse(SysRole.Admin)).ToList();
                        result = _mapper.Map<List<RoleDto>>(roles);
                        return result;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

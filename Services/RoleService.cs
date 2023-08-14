using AutoMapper;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
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
        public async Task<RoleDto> Get(string id)
        {
            return null;
        }
        public async Task<List<RoleDto>> GetAll()
        {
            return null;
        }
        public async Task<bool> Update(string id)
        {
            return true;
        }
        public async Task<bool> Delete(string id)
        {
            return true;
        }
    }
}

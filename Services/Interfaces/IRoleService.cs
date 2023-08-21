using WebApi.Models.Role;
using WebApi.Models.Roles;
using WebApi.Models.Users;

namespace WebApi.Services.Interfaces
{
    public interface IRoleService
    {
        public Task<string> Create(CreateRoleRequest request, UserClaims _claims);
        public Task<RoleDetailDto> Get(string id, UserClaims claims);
        public Task<List<RoleDto>> GetAll(UserClaims claims);
        public Task<bool> Update(string id, UpdateRoleRequest request, UserClaims claims);
        public Task<bool> Delete(string id, UserClaims claims);
    }
}

using WebApi.Models.Role;
using WebApi.Models.Roles;
using WebApi.Models.Users;

namespace WebApi.Services.Interfaces
{
    public interface IRoleService
    {
        public Task<string> Create(CreateRoleRequest request, UserClaims _claims);
        public Task<RoleDto> Get(string id);
        public Task<List<RoleDto>> GetAll();
        public Task<bool> Update(string id);
        public Task<bool> Delete(string id);
    }
}

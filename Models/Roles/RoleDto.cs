using WebApi.Models.Permissions;

namespace WebApi.Models.Role
{
    public class RoleDto
    {
        public string Name { get; set; }

        public string Id { get; set; }
    }

    public class RoleDetailDto : RoleDto
    {
        public List<RolePermissionDto> Permissions { get; set; }
    }
}

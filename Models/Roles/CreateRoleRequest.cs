using System.ComponentModel.DataAnnotations;
using WebApi.Models.Permissions;

namespace WebApi.Models.Roles
{
    public class CreateRoleRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public List<PermissionDto> Permissions { get; set; }
    }
}

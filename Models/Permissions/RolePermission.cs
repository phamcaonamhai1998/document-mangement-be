using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Permissions
{
    public class RolePermissionDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }
        
    }
}

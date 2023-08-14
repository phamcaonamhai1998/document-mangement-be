using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Permissions
{
    public class PermissionDto
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        [Required]
        public string Code { get; set; }
        
        [Required]
        public string GroupCode { get; set; }
            
    }
}

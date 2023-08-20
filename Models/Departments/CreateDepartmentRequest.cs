using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Departments;

public class CreateDepartmentRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Phone { get; set; }

    public string OrgId { get; set; }
}

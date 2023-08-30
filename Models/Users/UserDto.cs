using WebApi.Entities;
using WebApi.Models.Departments;
using WebApi.Models.Organizations;
using WebApi.Models.Role;

namespace WebApi.Models.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string AvatarUrl { get; set; }
    public string Phone { get; set; }
    public OrganizationDto Org{ get; set; }
    public DepartmentDto Department { get; set; }
    public RoleDto Role { get; set; }
}

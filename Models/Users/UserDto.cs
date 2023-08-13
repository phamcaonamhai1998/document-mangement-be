using WebApi.Entities;

namespace WebApi.Models.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string AvatarUrl { get; set; }
    public string Phone { get; set; }
    public Organization Org{ get; set; }
    public Department Department { get; set; }
    public Role Role { get; set; }
}

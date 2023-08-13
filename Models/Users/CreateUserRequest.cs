using WebApi.Entities;

namespace WebApi.Models.Users;

public class CreateUserRequest
{   
    public bool IsActive { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string AvatarUrl { get; set; }
    public string Phone { get; set; }
    public string OrgId { get; set; }
    public Department Department { get; set; }
    public List<AccountRoles> AccountRoles { get; set; } = new();
}

namespace WebApi.Entities;

public class Account: BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string AvatarUrl { get; set; }
    public string Phone { get; set; }
    public string OrgId { get; set; }
    public bool IsActive { get; set; }
    public Department Department { get; set; }
    public List<AccountRoles> AccountRoles { get; set; } = new();
}
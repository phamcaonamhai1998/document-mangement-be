namespace WebApi.Entities;

public class Role: BaseEntity
{
    public string Name { get; set; }

    public string Code { get; set; }

    public string OrgId { get; set; }

    public List<AccountRoles> AccountRoles { get; set; } = new();

}
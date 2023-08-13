namespace WebApi.Models.Departments;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string orgId {get; set;}
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}
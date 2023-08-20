
using WebApi.Models.Organizations;

namespace WebApi.Models.Departments;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string OrganizationId {get; set;}
    public string DepartmentDriveFolderId { get; set; }

    public OrganizationDto Organization{ get; set; }
}
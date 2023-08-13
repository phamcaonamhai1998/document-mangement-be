using WebApi.Entities;
using WebApi.Models.Role;

namespace WebApi.Models.Organizations;

public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}
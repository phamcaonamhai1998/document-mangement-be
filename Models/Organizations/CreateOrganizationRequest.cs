using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Organizations;

public class CreateOrganizationRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Phone { get; set; }
}

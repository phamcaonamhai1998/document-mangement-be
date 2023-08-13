namespace WebApi.Models.Organizations;
public class CreateOrganizationResponse
{
    public CreateOrganizationResponse(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}

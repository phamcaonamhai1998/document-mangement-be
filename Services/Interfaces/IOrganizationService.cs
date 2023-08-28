using WebApi.Models.Organizations;
using WebApi.Models.Users;

namespace WebApi.Services.Interfaces;

public interface IOrganizationService
{
    public Task<List<OrganizationDto>> GetAll();
    public Task<CreateOrganizationResponse> Create(CreateOrganizationRequest payload);
    public Task<bool> Delete(string id);
    public Task<bool> Update(string id, UpdateOrganizationRequest payload);
    public Task<OrganizationDto> GetById(string id);

    public Task<List<OrganizationDto>> GetAvailableOrgToCreateOwner(UserClaims claims);
}

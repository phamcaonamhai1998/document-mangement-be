using WebApi.Models.Departments;
using WebApi.Models.Users;

namespace WebApi.Services.Interfaces;

public interface IDepartmentService
{
    public Task<List<DepartmentDto>> GetAll(UserClaims claim);
    public Task<CreateDepartmentResponse> Create(CreateDepartmentRequest payload, UserClaims claim);
    public Task<bool> Delete(string id, UserClaims claim);
    public Task<bool> Update(string id, UpdateDepartmentRequest payload, UserClaims claim);
    public Task<DepartmentDto> GetById(string id, UserClaims claim);

    public Task<List<DepartmentDto>> GetOrgDeps(UserClaims claim);
    public Task<List<DepartmentDto>> GetDepsByOrgId(string orgId);
}

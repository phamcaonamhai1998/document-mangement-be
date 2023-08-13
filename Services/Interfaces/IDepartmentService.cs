using WebApi.Models.Departments;

namespace WebApi.Services.Interfaces;

public interface IDepartmentService
{
    public Task<List<DepartmentDto>> GetAll();
    public Task<CreateDepartmentResponse> Create(CreateDepartmentRequest payload);
    public Task<bool> Delete(string id);
    public Task<bool> Update(string id, UpdateDepartmentRequest payload);
    public Task<DepartmentDto> GetById(string id);
}

using WebApi.Models.Procedures;
using WebApi.Models.Users;

namespace WebApi.Services.Interfaces;

public interface IProcedureService
{
    public Task<List<ProcedureDto>> GetAll(UserClaims claims, ProcedureQuery query);
    public Task<List<ProcedureDto>> GetOrgProcedures(UserClaims claims, ProcedureQuery query);
    public Task<List<ProcedureDto>> GetDepartmentProcedures(UserClaims claims, ProcedureQuery query);
    public Task<CreateProcedureResponse> Create(CreateProcedureRequest payload, UserClaims _claims);
    public Task<bool> Delete(string id);
    public Task<bool> Update(string id, UpdateProcedureRequest payload, UserClaims _claims);
    public Task<ProcedureDto> GetById(string id);
}

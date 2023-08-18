using WebApi.Models.Procedures;
using WebApi.Models.Users;

namespace WebApi.Services.Interfaces;

public interface IProcedureStepService
{
    public Task<ProcedureDto> GetStepsByProcedureId(string id);
}
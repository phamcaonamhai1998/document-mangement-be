using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Validations;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Procedures;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class ProcedureService : IProcedureService
{
    private readonly DataContext _dbContext;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;

    public ProcedureService(DataContext dbContext, IJwtUtils jwtUtils, IMapper mapper, IOptions<AppSettings> appSettings)
    {
        _dbContext = dbContext;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }

    public async Task<CreateProcedureResponse> Create(CreateProcedureRequest payload, UserClaims _claims)
    {
        Organization org = _dbContext.Organizations.SingleOrDefault(a => a.Id == _claims.Organization.Id);
        Procedure procedure = new Procedure(Guid.NewGuid(), payload.Name);
        procedure.Organization = org;
        procedure.DepartmentId = payload.DepartmentId;
        _dbContext.Procedures.Add(procedure);

        if ( payload.ProcedureSteps.Count() > 0){
            HandleProcedureStepDto dto = new HandleProcedureStepDto();
            dto.ProcedureId = procedure.Id;
            dto.ProcedureStepItems = payload.ProcedureSteps;
            await _handleProcedureSteps(dto, procedure);
        };
        _dbContext.SaveChanges();


        return new CreateProcedureResponse(procedure.Id);
    }

    private async Task<bool> _handleProcedureSteps(HandleProcedureStepDto payload,Procedure procedure)
    {

        for(int i = 0; i < payload.ProcedureStepItems.Count; i ++){
            ProcedureStep step = new ProcedureStep(Guid.NewGuid(), i, payload.ProcedureStepItems[i].Description, payload.ProcedureStepItems[i].AssignId);
            step.Procedure = procedure;
            _dbContext.ProcedureSteps.Add(step);
        }
        _dbContext.SaveChanges();
        return true;
    }

    public Task<bool> Delete(string id)
    {
         if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }

        var proc = _dbContext.Organizations.SingleOrDefault(a => a.Id == Guid.Parse(id));

        if (proc == null) throw new Exception("procedure_is_not_found");

        _dbContext.Organizations.Remove(proc);
        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }

    public Task<List<ProcedureDto>> GetAll()
    {
        var procedures = _dbContext.Procedures.ToList();
        List<ProcedureDto> procDtos = new List<ProcedureDto>();
        procedures.ForEach((pro) =>
        {
            var procDto = _mapper.Map<ProcedureDto>(pro);
            procDtos.Add(procDto);
        });
        return Task.FromResult(procDtos);
    }

    public Task<ProcedureDto> GetById(string id)
    {
        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }
        var proc = _dbContext.Procedures.SingleOrDefault(a => a.Id == Guid.Parse(id));
        ProcedureDto procDto = _mapper.Map<ProcedureDto>(proc);

        return Task.FromResult(procDto);
    }

    public Task<bool> Update(string id, UpdateProcedureRequest payload, UserClaims _claims)
    {
        if(payload == null)
        {
            throw new Exception("payload_is_empty");
        }

        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }

        Procedure procedure = _dbContext.Procedures.SingleOrDefault(a => a.Id == Guid.Parse(id));


        procedure.Name = payload.Name;
        if( payload.ProcedureSteps.Count() > 0){
            HandleProcedureStepDto dto = new HandleProcedureStepDto();
            dto.ProcedureStepItems = payload.ProcedureSteps;
            _handleProcedureSteps(dto, procedure);
        };
        
        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }
}
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Validations;
using System.Diagnostics;
using WebApi.Authorization;
using WebApi.Common.Constants;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Documents;
using WebApi.Models.Procedures;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        procedure.CreatedBy = _claims.Id;
        procedure.IsActive = false;
        _dbContext.Procedures.Add(procedure);

        if (payload.ProcedureSteps.Count() > 0)
        {
            HandleProcedureStepDto dto = new HandleProcedureStepDto();
            dto.ProcedureId = procedure.Id;
            dto.ProcedureStepItems = payload.ProcedureSteps;
            await _handleProcedureSteps(dto, procedure, _claims, false);
        };
        _dbContext.SaveChanges();


        return new CreateProcedureResponse(procedure.Id);
    }

    private async Task<bool> _handleProcedureSteps(HandleProcedureStepDto payload, Procedure procedure, UserClaims _claims, bool isUpdate)
    {
        if (isUpdate)
        {
            var procSteps = _dbContext.ProcedureSteps.Where(ps => ps.Procedure.Id == procedure.Id).ToList();

            if (procSteps.Count() > 0)
            {
                procSteps.ForEach(ps => _dbContext.ProcedureSteps.Remove(ps));
            }
            _dbContext.SaveChanges();
        }

        for (int i = 0; i < payload.ProcedureStepItems.Count; i++)
        {
            ProcedureStep step = new ProcedureStep(Guid.NewGuid(), i + 1, payload.ProcedureStepItems[i].Description, payload.ProcedureStepItems[i].AssignId);
            step.Procedure = procedure;
            if (isUpdate == true)
            {
                step.UpdatedBy = _claims.Id;
            }
            else
            {
                step.CreatedBy = _claims.Id;
            }
            _dbContext.ProcedureSteps.Add(step);
        }
        _dbContext.SaveChanges();
        return true;
    }

    public Task<bool> Delete(string id)
    {
        try
        {
            if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
            {
                throw new Exception("id_is_empty");
            }

            var proc = _dbContext.Procedures.SingleOrDefault(a => a.Id == Guid.Parse(id));

            if (proc == null || proc.IsActive == true) throw new Exception("procedure_is_not_found_or_in_active");

            _dbContext.Procedures.Remove(proc);
            _dbContext.SaveChanges();
            return Task.FromResult(true);

        }
        catch (Exception err)
        {
            Console.WriteLine(err);
            return null;
        }
    }

    public Task<List<ProcedureDto>> GetAll(UserClaims claims, ProcedureQuery query)
    {

        //if (claims.Role != null && claims.Role.Id.ToString() != SysRole.Admin)
        //{
        //    return Task.FromResult(new List<ProcedureDto>());
        //}
        //var cmd = _dbContext.Procedures;

        ////if (query.IsActive)
        ////{
        ////    cmd.Where(d => d.IsActive == true);
        ////}

        //var procedures = cmd.ToList();
        //List<ProcedureDto> procDtos = new List<ProcedureDto>();
        //procedures.ForEach((pro) =>
        //{
        //    var procDto = _mapper.Map<ProcedureDto>(pro);
        //    procDtos.Add(procDto);
        //});
        //return Task.FromResult(procDtos);

        var command = _dbContext.Procedures.Where(p => p.Id != null);

        if (claims.Rights.Any(r => r == $"{PermissionGroupCode.Procedure}:{PermissionCode.List}"))
        {
            if (claims.Department != null && !string.IsNullOrWhiteSpace(claims.Department.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Department.Id.ToString()))
            {
                command = command.Where(p => p.DepartmentId != null && p.DepartmentId== claims.Department.Id.ToString());
            }
            else if (claims.Organization != null && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && claims.Organization.Id.ToString() != SystemOrg.SystemOrgId)
            {
                command = command.Where(r => r.Organization.Id == claims.Organization.Id);
            }
        }

        var procedures = command.ToList();
        List<ProcedureDto> procDtos = new List<ProcedureDto>();
        procedures.ForEach((pro) =>
        {
            var procDto = _mapper.Map<ProcedureDto>(pro);
            procDtos.Add(procDto);
        });
        return Task.FromResult(procDtos);
    }
    public Task<List<ProcedureDto>> GetOrgProcedures(UserClaims claims, ProcedureQuery query)
    {
        if (claims.Organization != null && claims.Organization.Id.ToString().IsNullOrEmpty())
        {
            return Task.FromResult(new List<ProcedureDto>());
        }

        var cmd = _dbContext.Procedures.Where(d => d.Organization.Id == claims.Organization.Id);
        //if (query.IsActive)
        //{
        //    cmd.Where(d => d.IsActive == true);
        //}

        var docs = cmd.ToList();
        return Task.FromResult(_mapper.Map<List<ProcedureDto>>(docs));
    }

    public Task<List<ProcedureDto>> GetDepartmentProcedures(UserClaims claims, ProcedureQuery query)
    {

        if (claims.Department != null && claims.Department.Id.ToString().IsNullOrEmpty())
        {
            return Task.FromResult(new List<ProcedureDto>());
        }

        var cmd = _dbContext.Procedures.Where(p => Guid.Parse(p.DepartmentId) == claims.Department.Id);
        //if (query.IsActive)
        //{
        //    cmd.Where(d => d.IsActive == true);
        //}
        var docs = cmd.ToList();

        return Task.FromResult(_mapper.Map<List<ProcedureDto>>(docs));
    }
    public async Task<List<ProcedureDto>> GetAvailableProcs(UserClaims claims, ProcedureQuery query)
    {
        var result = new List<ProcedureDto>();

        if (claims.Department != null && !string.IsNullOrWhiteSpace(claims.Department.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Department.Id.ToString()))
        {
            var procs = _dbContext.Procedures.Where(p => p.DepartmentId == claims.Department.Id.ToString()).ToList();
            result = _mapper.Map<List<ProcedureDto>>(procs);
        }

        else if (claims.Organization != null && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && !string.IsNullOrWhiteSpace(claims.Organization.Id.ToString()) && claims.Organization.Id.ToString() != SystemOrg.SystemOrgId)
        {
            var procs = _dbContext.Procedures.Where(p => p.Organization != null && p.Organization.Id.ToString() == claims.Organization.Id.ToString()).ToList();
            result = _mapper.Map<List<ProcedureDto>>(procs);
        }

        return result;
    }

    public Task<ProcedureDto> GetById(string id)
    {
        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }
        var proc = _dbContext.Procedures.SingleOrDefault(a => a.Id == Guid.Parse(id));
        ProcedureDto procDto = _mapper.Map<ProcedureDto>(proc);

        var steps = _dbContext.ProcedureSteps.Where(step => step.Procedure.Id == proc.Id).ToList();
        var stepDtos = _mapper.Map<List<ProcedureStepDto>>(steps);

        if (stepDtos?.Count() > 0)
        {
            procDto.ProcedureSteps = stepDtos;
        }
        return Task.FromResult(procDto);
    }

    public async Task<bool> Update(string id, UpdateProcedureRequest payload, UserClaims _claims)
    {
        if (payload == null)
        {
            throw new Exception("payload_is_empty");
        }

        if (String.IsNullOrEmpty(id) || String.IsNullOrWhiteSpace(id))
        {
            throw new Exception("id_is_empty");
        }
        Procedure procedure = _dbContext.Procedures.SingleOrDefault(a => a.Id == Guid.Parse(id));

        if (procedure.IsActive) throw new Exception("proc_is_active");

        procedure.UpdatedBy = _claims.Id;
        procedure.Name = payload.Name;
        if (payload.ProcedureSteps.Count() > 0)
        {
            HandleProcedureStepDto dto = new HandleProcedureStepDto();
            dto.ProcedureStepItems = payload.ProcedureSteps;
            await _handleProcedureSteps(dto, procedure, _claims, true);
        };

        _dbContext.SaveChanges();
        return true;
    }
}
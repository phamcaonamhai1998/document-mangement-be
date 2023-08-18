using AutoMapper;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Procedures;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class ProcedureStepService : IProcedureStepService
{
    private readonly DataContext _dbContext;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;

    public ProcedureStepService(DataContext dbContext, IJwtUtils jwtUtils, IMapper mapper, IOptions<AppSettings> appSettings)
    {
        _dbContext = dbContext;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }
    public Task<ProcedureDto> GetStepsByProcedureId(string id)
    {
        List<ProcedureStep> steps = _dbContext.ProcedureSteps.Where(rp => rp.Procedure.Id == Guid.Parse(id)).ToList();
        if(steps?.Count() == 0){
            return null;
        }
        return null;
    }
}
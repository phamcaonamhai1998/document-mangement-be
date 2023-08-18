using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Procedures;

public class HandleProcedureStepDto
{
    public Guid ProcedureId {get; set;}
    public List<CreateProcedureStepItemDto> ProcedureStepItems {get; set;}
}


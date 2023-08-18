using System.ComponentModel.DataAnnotations;
using WebApi.Entities;

namespace WebApi.Models.Procedures;

public class CreateProcedureRequest
{
    [Required]
    public string Name { get; set; }

    public string DepartmentId {get; set;}

    [Required]
    public List<CreateProcedureStepItemDto> ProcedureSteps {get; set;} 

}


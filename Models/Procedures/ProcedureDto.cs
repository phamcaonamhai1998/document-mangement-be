namespace WebApi.Models.Procedures;

public class ProcedureDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public string departmentId { get; set; }
    public List<ProcedureStepDto> ProcedureSteps { get; set; }
}
namespace WebApi.Models.Procedures;

public class ProcedureStepDto
{
    public Guid Id { get; set; }
    public int Priority { get; set; }
    public string Description { get; set; }
    public string AssignId {get; set;}
}
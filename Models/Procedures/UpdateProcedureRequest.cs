namespace WebApi.Models.Procedures
{
    public class UpdateProcedureRequest
    {
        public string Name { get; set; }
        public List<CreateProcedureStepItemDto> ProcedureSteps {get; set;} 
    }
}

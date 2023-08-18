namespace WebApi.Models.Procedures;
public class CreateProcedureResponse
{
    public CreateProcedureResponse(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}

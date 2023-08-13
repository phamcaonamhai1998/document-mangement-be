namespace WebApi.Models.Departments;
public class CreateDepartmentResponse
{
    public CreateDepartmentResponse(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}

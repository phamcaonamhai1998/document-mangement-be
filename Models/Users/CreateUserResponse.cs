namespace WebApi.Models.Users;
public class CreateUserResponse
{
    public CreateUserResponse(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}

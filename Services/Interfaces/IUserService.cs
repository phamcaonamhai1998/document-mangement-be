using WebApi.Models.Users;

namespace WebApi.Services.Interfaces;

public interface IUserService
{
    public Task<CreateUserResponse> Create(CreateUserRequest payload);

    public Task<bool> Delete(string id);
}

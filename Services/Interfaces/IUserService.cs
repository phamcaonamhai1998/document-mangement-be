using WebApi.Models.Users;

namespace WebApi.Services.Interfaces;

public interface IUserService
{
    Task<CreateUserResponse> create(CreateUserRequest payload);
}

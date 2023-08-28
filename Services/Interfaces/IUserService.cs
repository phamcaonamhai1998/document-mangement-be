using Microsoft.AspNetCore.Mvc;
using WebApi.Models.Auth;
using WebApi.Models.Users;

namespace WebApi.Services.Interfaces;

public interface IUserService
{
    public Task<List<UserDto>> GetAll();
    public Task<List<UserDto>> GetOrgUsers(UserClaims claims);
    public Task<List<UserDto>> GetDepUsers(UserClaims claims);
    public Task<List<UserDto>> GetUsersCanAssign(UserClaims claims);
    public Task<CreateUserResponse> Create(CreateUserRequest payload);
    public Task<bool> Delete(string id);
    public Task<bool> Update(string id, UpdateUserRequest payload);
    public Task<UserDto> GetById(string id);    
    public Task<LoginResponse> Login(LoginRequest payload);
    public Task<bool> ChangePassword(ChangePasswordRequest payload, UserClaims claims);
}

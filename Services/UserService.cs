using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class UserService : IUserService
{
    private readonly DataContext _dbContext;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;

    public UserService(DataContext dbContext, IJwtUtils jwtUtils, IMapper mapper, IOptions<AppSettings> appSettings)
    {
        _dbContext = dbContext;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }

    public async Task<CreateUserResponse> Create(CreateUserRequest payload)
    {
        var account = _dbContext.Accounts.SingleOrDefault(x => x.Email == payload.Email);
        if (account != null) throw new Exception("user_email_is_existed");

        var createAccount = _mapper.Map<Account>(payload);

        return null;
    }
}

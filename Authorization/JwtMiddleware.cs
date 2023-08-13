namespace WebApi.Authorization;

using Microsoft.Extensions.Options;
using WebApi.Helpers;
using WebApi.Models.Users;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AppSettings _appSettings;

    public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
    {
        _next = next;
        _appSettings = appSettings.Value;
    }

    public async Task Invoke(HttpContext context, DataContext dataContext, IJwtUtils jwtUtils)
    {
        string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        UserClaims claims = jwtUtils.ValidateJwtToken(token);
        if (claims != null)
        {
            // attach account to context on successful jwt validation
            context.Items["Claims"] = claims;
        }

        await _next(context);
    }
}
namespace WebApi.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Entities;
using WebApi.Models.Users;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly IList<string> _permissions;

    public AuthorizeAttribute(params string[] permissions)
    {
        _permissions = permissions;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // skip authorization if action is decorated with [AllowAnonymous] attribute
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
            return;

        // authorization
        var claims = (UserClaims)context.HttpContext.Items["Claims"];
        if (claims == null || (_permissions.Any() && !_permissions.Any(p => claims.Rights.Any(r => r == p))))
        {
            // not logged in or role not authorized
            context.Result = new JsonResult(new { message = "Forbidden" }) { StatusCode = StatusCodes.Status403Forbidden };
            return;
        }

        return;

    }
}
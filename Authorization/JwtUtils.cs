namespace WebApi.Authorization;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using Json.Net;
using WebApi.Models.Role;

public interface IJwtUtils
{
    public string GenerateJwtToken(UserClaims claims);
    public UserClaims ValidateJwtToken(string token);
}

public class JwtUtils : IJwtUtils
{
    private readonly DataContext _context;
    private readonly AppSettings _appSettings;

    public JwtUtils(
        DataContext context,
        IOptions<AppSettings> appSettings)
    {
        _context = context;
        _appSettings = appSettings.Value;
    }

    public string GenerateJwtToken(UserClaims claims)
    {
        try
        {
            // generate token that is valid for 15 minutes
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                new Claim("id", claims.Id.ToString()),
                new Claim("email", claims.Email.ToString()),
                new Claim("org", JsonNet.Serialize(claims.Organization)),
                new Claim("department",JsonNet.Serialize(claims.Department)),
                new Claim("role", JsonNet.Serialize(claims.Role)),
                new Claim("rights", JsonNet.Serialize(claims.Rights)),
                new Claim("firstName", claims.FirstName.ToString()),
                new Claim("lastName", claims.LastName.ToString()),
            }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
            return null;
        }

    }

    public UserClaims? ValidateJwtToken(string token)
    {
        if (token == null)
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            Guid UserId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            string firstName = jwtToken.Claims.First(x => x.Type == "firstName").Value;
            string lastName = jwtToken.Claims.First(x => x.Type == "lastName").Value;
            string email = jwtToken.Claims.First(x => x.Type == "email").Value;
            RoleDto role = JsonNet.Deserialize<RoleDto>(jwtToken.Claims.First(x => x.Type == "role").Value);
            Organization org = JsonNet.Deserialize<Organization>(jwtToken.Claims.First(x => x.Type == "org").Value);
            Department dep = JsonNet.Deserialize<Department>(jwtToken.Claims.First(x => x.Type == "department").Value);
            List<string> rights = JsonNet.Deserialize<List<string>>(jwtToken.Claims.First(x => x.Type == "rights").Value);

            // return claim
            return new UserClaims(UserId, firstName, lastName, role, dep, org, rights, email);
        }
        catch
        {
            // return null if validation fails
            return null;
        }
    }
}
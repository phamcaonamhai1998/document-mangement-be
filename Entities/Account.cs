namespace WebApi.Entities;

public class Account: BaseEntity
{
    public string Title { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string VerificationToken { get; set; }
    public DateTime? Verified { get; set; }
    public bool IsVerified => Created.HasValue || PasswordReset.HasValue;
    public string ResetToken { get; set; }
    public DateTime? ResetTokenExpires { get; set; }
    public DateTime? PasswordReset { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Updated { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; }

    public string AvatarUrl { get; set; }
    public string OrgId { get; set; }

    public Department Department { get; set; }

    public List<AccountRoles> AccountRoles { get; set; } = new();


    public bool OwnsToken(string token) 
    {
        return this.RefreshTokens?.Find(x => x.Token == token) != null;
    }
}
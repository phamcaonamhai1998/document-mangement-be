using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using WebApi.Common.Enum;

namespace WebApi.Entities;
public class Account: BaseEntity
{
    public static void ConfigurationEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Account>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
        modelBuilder.Entity<Account>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string AvatarUrl { get; set; }
    public string Phone { get; set; }
    public string OrgId { get; set; }
    public bool IsActive { get; set; }
    public AccountTypeEnum AccountType { get; set; }
    public Role Role { get; set; }
    public Department Department { get; set; }
}
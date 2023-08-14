using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities;

public class Role : BaseEntity
{
    public static void ConfigurationEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
        modelBuilder.Entity<Role>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    }

    public Role() { }

    public Role(Guid id, string name, string orgId)
    {
        Id = id;
        Name = name;
        OrgId = orgId;
    }

    public string Name { get; set; }

    public string OrgId { get; set; }
}
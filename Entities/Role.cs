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

    public Role(Guid id, string name, string orgId, string departmentId)
    {
        Id = id;
        Name = name;
        OrgId = orgId;
        DepartmentId = departmentId;
    }

    public string Name { get; set; }
    public string OrgId { get; set; }
    public string DepartmentId { get; set; }
}
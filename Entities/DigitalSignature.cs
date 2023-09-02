using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities;

public class DigitalSignature: BaseEntity
{
    public static void ConfigurationEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DigitalSignature>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
        modelBuilder.Entity<DigitalSignature>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    }

    public DigitalSignature() { }

    public string Path { get; set; }
    public string Name { get; set; }
    public Account User { get; set; }
    public bool IsDefault { get; set; }
    public string FileId { get; set; }
    public string HashPassword { get; set; }
}

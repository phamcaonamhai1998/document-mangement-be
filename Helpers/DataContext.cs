namespace WebApi.Helpers;

using Microsoft.EntityFrameworkCore;
using WebApi.Entities;

public class DataContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<AccountRoles> AccountRoles { get; set; }
    public DbSet<Organization> Organization { get; set; }
    public DbSet<Department> Department{ get; set; }

    private readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sqlite database
        options.UseNpgsql(Configuration.GetConnectionString("WebApiDatabase"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Role>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<AccountRoles>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Organization>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Department>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    }
}
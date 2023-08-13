namespace WebApi.Helpers;

using Microsoft.EntityFrameworkCore;
using WebApi.Entities;

public class DataContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Role>  Roles{ get; set; }
    public DbSet<Organization> Organizations{ get; set; }
    public DbSet<Department> Departments{ get; set; }
    public DbSet<RolePermission> RolePermissions{ get; set; }

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
        Account.ConfigurationEntity(modelBuilder);
        Role.ConfigurationEntity(modelBuilder);
        Organization.ConfigurationEntity(modelBuilder);
        Department.ConfigurationEntity(modelBuilder);
        RolePermission.ConfigurationEntity(modelBuilder);
    }
}
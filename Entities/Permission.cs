using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities
{
    public class Permission : BaseEntity
    {
        public static void ConfigurationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<Permission>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        }

        public Permission(string name, string groupCode, string code)
        {
            Code = code;
            GroupCode = groupCode;
            Name = name;
        }

        public string Code { get; set; }

        public string GroupCode { get; set; }

        public string Name { get; set; }

    }
}

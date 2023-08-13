using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities
{
    public class RolePermission: BaseEntity
    {
        public static void  ConfigurationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RolePermission>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<RolePermission>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        }

        public Guid RoleId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

    }
}

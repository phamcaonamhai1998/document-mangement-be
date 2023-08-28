using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities
{
    public class Department: BaseEntity
    {
        public static void ConfigurationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Department>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<Department>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        }

        public Department() { }

        public Department(Guid id, string name) {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string DepartmentDriveFolderId { get; set; }

        public Organization Organization { get; set; }

        public List<Account> Users { get; set; }
    }
}

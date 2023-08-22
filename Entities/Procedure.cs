    using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities
{
    public class Procedure : BaseEntity
    {

        public static void ConfigurationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Procedure>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<Procedure>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        }

        public Procedure(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public string DepartmentId { get; set; }
        public bool IsActive { get; set; }

        public Organization Organization { get; set; }

        public List<ProcedureStep> ProcedureSteps { get; set; }



    }
}

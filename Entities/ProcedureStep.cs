using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities
{
    public class ProcedureStep: BaseEntity
    {
        public static void ConfigurationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcedureStep>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<ProcedureStep>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        }

        public string Description { get; set; }

        public int Priority { get; set; }

        public Guid AssignId { get; set; }

        public Procedure Procedure { get; set; }

        public List<DocumentProcedureStep> DocumentProcedureSteps { get; set; }
    }
}

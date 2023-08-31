using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities
{
    public class DocumentProcedureStep: BaseEntity
    {
        public static void ConfigurationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentProcedureStep>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<DocumentProcedureStep>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        }

        public DocumentProcedureStep(Guid procedureId, string status)
        {
            ProcedureId = procedureId;
            Status = status;
        }

        public DocumentProcedureStep(DocumentProcedureStep ds)
        {
            ProcedureId = ds.ProcedureId;
            ProcedureStep = ds.ProcedureStep;
            Status = ds.Status;
            RejectReason = ds.RejectReason;
        }

        public Document Document { get; set; }

        public Guid ProcedureId { get; set; }
        public ProcedureStep ProcedureStep { get; set; }

        public string Status { get; set; }

        public string RejectReason { get; set; }

    }
}

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

        public DocumentProcedureStep(Guid documentId, Guid procedureId, string status)
        {
            DocumentId = documentId;
            ProcedureId = procedureId;
            Status = status;
        }   

        public Guid DocumentId { get; set; }

        public Guid ProcedureId { get; set; }
        public Guid ProcedureStepId { get; set; }

        public string Status { get; set; }

        public string RejectReason { get; set; }

    }
}

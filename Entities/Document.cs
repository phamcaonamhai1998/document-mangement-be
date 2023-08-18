using Microsoft.EntityFrameworkCore;
using WebApi.Common.Constants;

namespace WebApi.Entities
{
    public class Document: BaseEntity
    {
        public static void ConfigurationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<Document>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Document>().Property(b => b.Status).HasDefaultValueSql(DocumentStatus.PROCESSING);
        }

        public Document(Guid userId, string title, string path, bool isActive, string driveDocId, string description, Guid departmentId, Guid orgId) {
            UserId = userId;            
            Title = title;            
            Path = path;            
            IsActive = isActive;
            DriveDocId = driveDocId;
            DepartmentId = departmentId;
            OrgId = orgId;
            Description = description;
        }

        public Guid UserId { get; set; }

        public string Title { get; set; }

        public string Path { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public Guid OrgId { get; set; }

        public Guid DepartmentId { get; set; }

        public string DriveDocId { get; set; }

        public string Status { get; set; }  

        public List<DocumentProcedureStep> DocumentProcedureSteps { get; set; }

        public Procedure Procedure { get; set; }
    }
}

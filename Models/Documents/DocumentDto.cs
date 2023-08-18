using WebApi.Entities;

namespace WebApi.Models.Documents
{
    public class DocumentDto
    {
        public Guid UserId { get; set; }

        public string Title { get; set; }

        public string Path { get; set; }

        public bool IsActive { get; set; }

        public Guid OrgId { get; set; }

        public Guid DepartmentId { get; set; }

        public string DriveDocId { get; set; }

        public List<DocumentProcedureStep> DocumentProcedureSteps { get; set; }

        public Procedure Procedure { get; set; }
    }
}

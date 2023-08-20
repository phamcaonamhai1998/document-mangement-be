using WebApi.Entities;

namespace WebApi.Models.Documents
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Title { get; set; }

        public string Path { get; set; }

        public bool IsActive { get; set; }

        public Guid OrgId { get; set; }

        public Guid DepartmentId { get; set; }

        public string Description { get; set; }

        public string DriveDocId { get; set; }

        public List<DocumentProcedureStep> DocumentProcedureSteps { get; set; }

        public Procedure Procedure { get; set; }

        public string OrgName { get; set; }

        public string DepartmentName { get; set; }
        
        public string ProcedureName { get; set; }
        
        public string ProcedureId { get; set; }
        
        public string UserFullName { get; set; }
    }
}

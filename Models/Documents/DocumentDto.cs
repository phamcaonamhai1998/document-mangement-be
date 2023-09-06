using Nest;
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

        public string Status { get; set; }
    }

    public class DocumentProcedureStepDto : DocumentDto
    {
        public ProcedureStep Step { get; set; }
    }

    public class AssignDocumentDto
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
        public string OrgName { get; set; }
        public string DepartmentName { get; set; }
        public string ProcedureName { get; set; }
        public string ProcedureId { get; set; }
        public string UserFullName { get; set; }
        public string Status { get; set; }

        public AssignStepDto Step { get; set; }

    }

    public class AssignStepDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public Guid AssignId { get; set; }
        public Guid ProcedureId { get; set; }

        public string Status { get; set; }

    }
    
    public class DocStepDto
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string RejectReason { get; set; }
        public bool IsSigned { get; set; }
        public string DocSignedPath { get; set; }
        public string DocSignedId { get; set; }
        public string ProcedureId { get; set; }
        public string DocumentId { get; set; }
        public string ProcedureStepId { get; set; }
        public int Priority { get; set; }
    }
}
    
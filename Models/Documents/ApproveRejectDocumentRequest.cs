using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Documents
{
    public class ApproveDocumentRequest
    {
        [Required]
        public string ProcedureStepId { get; set; }

        public bool IsSign { get; set; }

        public string SignaturePassword { get; set; }

    }

    public class RejectDocumentRequest
    {
        [Required]
        public string ProcedureStepId { get; set; }

        public string Reason { get; set; }

    }
}

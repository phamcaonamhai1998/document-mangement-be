using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Documents
{
    public class CreateDocumentRequest
    {
        public string Title { get; set; }
        
        public string Description{ get; set; }

        [Required]
        public string DriveDocId { get; set; }

        [Required]
        public string ProcedureId { get; set; }

        public bool IsActive { get; set; }

    }
}

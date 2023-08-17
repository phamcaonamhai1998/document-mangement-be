using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Documents
{
    public class CreateDocumentRequest
    {
        [Required]
        public string Title { get; set; }
        
        public string Description{ get; set; }

        [Required]
        public string DriveDocId { get; set; }

        public bool IsActive { get; set; }

    }
}

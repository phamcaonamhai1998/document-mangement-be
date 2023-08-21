using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Documents
{
    public class UpdateDocProcedure
    {
        public string ProcedureId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string DriveDocId { get; set; }
    }
}

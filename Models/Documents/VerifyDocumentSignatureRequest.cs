namespace WebApi.Models.Documents
{
    public class VerifyDocumentSignatureRequest
    {
        public string DocId { get; set; }
        public string ProcedureStepId { get; set; }
    }
}

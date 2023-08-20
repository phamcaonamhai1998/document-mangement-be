using WebApi.Models.Documents;
using WebApi.Models.Users;

namespace WebApi.Services.Interfaces
{

    public interface IDocumentService
    {
        public Task<bool> Create(CreateDocumentRequest payload, UserClaims claims);
        public Task<bool> UpdateDoc(UpdateDocProcedure payload, string id, UserClaims claims);
        public Task<List<DocumentDto>> GetAll(UserClaims claims, GetDocumentsRequest query);
        public Task<List<DocumentDto>> GetUserDocs(UserClaims claims, GetDocumentsRequest query);
        public Task<List<DocumentDto>> GetOrgDocs(UserClaims claims, GetDocumentsRequest query);
        public Task<List<DocumentDto>> GetDepartmentDocs(UserClaims claims, GetDocumentsRequest query);
        public Task<DocumentDto> GetUserDoc(string id, UserClaims claims);
        public Task<bool> Delete(string id, UserClaims claims);
        public Task<bool> ApproveDocStep(ApproveDocumentRequest payload, string id, UserClaims claims);
        public Task<bool> RejectDocStep(RejectDocumentRequest payload, string id, UserClaims claims);
    }
}

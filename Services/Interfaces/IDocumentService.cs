using WebApi.Models.Documents;
using WebApi.Models.Users;

namespace WebApi.Services.Interfaces
{

    public interface IDocumentService
    {
        public Task<bool> Create(CreateDocumentRequest payload, UserClaims claims);
        public Task<List<DocumentDto>> GetAll(UserClaims claims);
        public Task<string> Get(string id, UserClaims claims);
        public Task<bool> Delete(string id, UserClaims claims);
    }
}

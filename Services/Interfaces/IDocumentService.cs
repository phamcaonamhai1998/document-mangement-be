﻿using WebApi.Models.Documents;
using WebApi.Models.Users;

namespace WebApi.Services.Interfaces
{

    public interface IDocumentService
    {
        public Task<bool> Create(CreateDocumentRequest payload, UserClaims claims);
        public Task<List<DocumentDto>> GetUserDocs(UserClaims claims);
        public Task<List<DocumentDto>> GetOrgDocs(UserClaims claims);
        public Task<DocumentDto> GetUserDoc(string id, UserClaims claims);
        public Task<bool> Delete(string id, UserClaims claims);
    }
}
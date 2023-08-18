using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Documents;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly DataContext _dbContext;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly StorageHelper _storageHelper;
        public DocumentService(DataContext dbContext, IMapper mapper, IOptions<AppSettings> appSettings, StorageHelper storageHelper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _storageHelper = storageHelper;
        }
        public async Task<bool> Create(CreateDocumentRequest payload, UserClaims claims)
        {
            //get webViewLink of document from drive
            var driveFile = await _storageHelper.GetFile(payload.DriveDocId);

            if (driveFile == null || string.IsNullOrEmpty(driveFile.Id))
            {
                throw new Exception("document_has_not_been_uploaded");
            }

            Document entity = new Document(claims.Id, payload.Title, driveFile.WebViewLink, payload.IsActive, payload.DriveDocId, claims.Department.Id, claims.Organization.Id);
            entity.CreatedAt = DateTime.UtcNow;
            _dbContext.Documents.Add(entity);
            _dbContext.SaveChanges();
            return true;
        }

        public async Task<List<DocumentDto>> GetUserDocs(UserClaims claims)
        {
            if (claims.Id.ToString().IsNullOrEmpty())
            {
                return new List<DocumentDto>();
            }

            var docs = _dbContext.Documents.Where(d => d.UserId == claims.Id).ToList();

            return _mapper.Map<List<DocumentDto>>(docs);
        }

        public async Task<List<DocumentDto>> GetOrgDocs(UserClaims claims)
        {
            if (claims.Organization != null && claims.Organization.Id.ToString().IsNullOrEmpty())
            {
                return new List<DocumentDto>();
            }

            var docs = _dbContext.Documents.Where(d => d.OrgId == claims.Organization.Id).ToList();

            return _mapper.Map<List<DocumentDto>>(docs);
        }

        public async Task<DocumentDto> GetUserDoc(string id, UserClaims claims)
        {
            var doc = _dbContext.Documents.SingleOrDefault(d => d.Id == Guid.Parse(id));

            if (doc == null || doc.UserId != claims.Id || doc.OrgId != claims.Organization.Id)
            {
                throw new Exception("document_not_found");
            }
            return _mapper.Map<DocumentDto>(doc);
        }
        public async Task<bool> Delete(string id, UserClaims claims)
        {
            var document = _dbContext.Documents.SingleOrDefault(d => d.Id == Guid.Parse(id));
            if (document == null)
            {
                throw new Exception("document_is_not_found");
            }

            if (document.DriveDocId != null)
            {
                await _storageHelper.DeleteFile(document.DriveDocId);
            }

            _dbContext.Documents.Remove(document);
            _dbContext.SaveChanges();
            return true;
        }
    }
}

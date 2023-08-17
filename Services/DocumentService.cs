using AutoMapper;
using Microsoft.Extensions.Options;
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

            Document entity = new Document(claims.Id, payload.Title, driveFile.WebViewLink, payload.IsActive, claims.Organization.Id);
            _dbContext.Documents.Add(entity);
            _dbContext.SaveChanges();
            return true;
        }

        public async Task<List<DocumentDto>> GetAll(UserClaims claims)
        {
            return null;
        }
        public async Task<string> Get(string id, UserClaims claims)
        {
            return "";
        }
        public async Task<bool> Delete(string id, UserClaims claims)
        {
            return true;
        }
    }
}

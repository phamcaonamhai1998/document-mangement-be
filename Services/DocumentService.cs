using AutoMapper;
using Google.Apis.Drive.v3.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Authorization;
using WebApi.Common.Constants;
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

            Procedure proc = _dbContext.Procedures.SingleOrDefault(proc => proc.Id == Guid.Parse(payload.ProcedureId));

            if (proc == null)
            {
                throw new Exception("procedure_is_not_found");
            }

            Document entity = new Document(claims.Id, payload.Title, driveFile.WebViewLink, payload.IsActive, payload.DriveDocId, payload.Description, claims.Department.Id, claims.Organization.Id);
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.Procedure = proc;
            entity.Status = DocumentStatus.PROCESSING;

            _dbContext.Documents.Add(entity);
            _dbContext.SaveChanges();

            await _HandleAssignDocToProcedureSteps(entity.Id, proc.Id);
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

        public async Task<List<DocumentDto>> GetAll(UserClaims claims)
        {
            if (claims.Role != null && claims.Role.Id.ToString() != SysRole.Admin)
            {
                return new List<DocumentDto>();
            }

            var docs = _dbContext.Documents.ToList();

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

        public async Task<List<DocumentDto>> GetDepartmentDocs(UserClaims claims)
        {
            if (claims.Department != null && claims.Department.Id.ToString().IsNullOrEmpty())
            {
                return new List<DocumentDto>();
            }

            var docs = _dbContext.Documents.Where(d => d.DepartmentId == claims.Department.Id).ToList();

            return _mapper.Map<List<DocumentDto>>(docs);
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

        public async Task<bool> UpdateDoc(UpdateDocProcedure payload, string id, UserClaims claims)
        {

            Document doc = _dbContext.Documents.SingleOrDefault(d => d.Id == Guid.Parse(id));

            if (doc == null)
            {
                throw new Exception("document_not_found");
            }

            // check doc can update
            // if one of doc steps has change status so can not update 
            if (payload.ProcedureId != null)
            {
                Procedure proc = _dbContext.Procedures.SingleOrDefault(proc => proc.Id == Guid.Parse(payload.ProcedureId));
                doc.Procedure = proc;

                List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Where(dps => dps.DocumentId == doc.Id).ToList();
                if (docSteps.Count() > 0 && !docSteps.All(ds => ds.Status == DocumentStepStatus.PROCESSING))
                {
                    throw new Exception("exist_step_status_change_of_document");
                }
            }

            if (payload.DriveDocId != null || !string.IsNullOrWhiteSpace(payload.DriveDocId) || !string.IsNullOrEmpty(payload.DriveDocId))
            {
                doc.DriveDocId = payload.DriveDocId;
                // get webViewLink of document from drive
                var driveFile = await _storageHelper.GetFile(payload.DriveDocId);
                if (driveFile.Id == null)
                {
                    throw new Exception("drive_document_is_not_found");
                }

                doc.Path = driveFile.WebViewLink;
            }

            doc.Title = payload.Title;
            doc.Description = payload.Description;
            doc.IsActive = payload.IsActive;


            _dbContext.Documents.Update(doc);
            _dbContext.SaveChanges();

            await _HandleAssignDocToProcedureSteps(doc.Id, doc.Procedure.Id);
            return true;
        }

        public async Task<bool> ApproveDocStep(ApproveDocumentRequest payload, string id, UserClaims claims)
        {
            DocumentProcedureStep docStep = _dbContext.DocumentProcedureSteps.SingleOrDefault((dps) => dps.Id == Guid.Parse(payload.ProcedureStepId));
            if (docStep == null)
            {
                throw new Exception("doc_step_is_not_found");
            }
            docStep.Status = DocumentStepStatus.APPROVED;
            _dbContext.DocumentProcedureSteps.Update(docStep);
            _dbContext.SaveChanges();

            //check if all doc steps is approved => change document to approved
            List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Where((dps) => dps.DocumentId == Guid.Parse(id)).ToList();
            bool isAllApproved = docSteps.All(ds => ds.Status == DocumentStepStatus.APPROVED);

            if (isAllApproved)
            {
                Document doc = _dbContext.Documents.SingleOrDefault(d => d.Id == Guid.Parse(id));
                doc.Status = DocumentStatus.PUBLISHED;
                _dbContext.Documents.Update(doc);
                _dbContext.SaveChanges();
            }

            return true;
        }
        public async Task<bool> RejectDocStep(RejectDocumentRequest payload, string id, UserClaims claims)
        {
            DocumentProcedureStep docStep = _dbContext.DocumentProcedureSteps.SingleOrDefault((dps) => dps.Id == Guid.Parse(payload.ProcedureStepId));
            if (docStep == null)
            {
                throw new Exception("doc_step_is_not_found");
            }

            docStep.Status = DocumentStepStatus.REJECTED;
            _dbContext.DocumentProcedureSteps.Update(docStep);
            _dbContext.SaveChanges();

            List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Where((dps) => dps.DocumentId == Guid.Parse(id) && dps.Status == DocumentStepStatus.PROCESSING).ToList();
            docSteps.ForEach(docStep =>
            {

                docStep.Status = DocumentStepStatus.REJECTED;
                _dbContext.DocumentProcedureSteps.Update(docStep);

            });

            _dbContext.SaveChanges();
            Document doc = _dbContext.Documents.SingleOrDefault(d => d.Id == Guid.Parse(id));
            doc.Status = DocumentStatus.REJECTED;
            _dbContext.Documents.Update(doc);
            _dbContext.SaveChanges();

            return true;
        }

        //pe-condition: all procedure steps of document must be processing
        private async Task<bool> _HandleAssignDocToProcedureSteps(Guid docId, Guid procedureId)
        {
            //remove old steps
            List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Where(ds => ds.DocumentId == docId).ToList();

            docSteps.ForEach(ds =>
            {
                _dbContext.DocumentProcedureSteps.Remove(ds);
            });

            _dbContext.SaveChanges();

            //add new steps
            List<ProcedureStep> steps = _dbContext.ProcedureSteps.Where(ps => ps.Procedure.Id == procedureId).ToList();
            steps.ForEach(step =>
            {
                _dbContext.DocumentProcedureSteps.Add(new DocumentProcedureStep(docId, procedureId, DocumentStepStatus.PROCESSING));
            });

            _dbContext.SaveChanges();
            return true;
        }
    }
}

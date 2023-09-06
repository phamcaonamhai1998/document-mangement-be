using AutoMapper;
using Docnet.Core;
using Docnet.Core.Models;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nest;
using Org.BouncyCastle.Crypto.Digests;
using WebApi.Common.Constants;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.DigitalSignature;
using WebApi.Models.Documents;
using WebApi.Models.ElasticSearch;
using WebApi.Models.Users;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class DocumentService : IDocumentService
{
    private readonly DataContext _dbContext;
    private readonly IMapper _mapper;
    private readonly StorageHelper _storageHelper;
    private readonly ElasticSearchHelper _elasticSearchHelper;
    private readonly DigitalSignHelper _digitalSignHelper;
    public DocumentService(DataContext dbContext, IMapper mapper, StorageHelper storageHelper, ElasticSearchHelper elasticSearchHelper, DigitalSignHelper digitalSignHelper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _storageHelper = storageHelper;
        _elasticSearchHelper = elasticSearchHelper;
        _digitalSignHelper = digitalSignHelper;
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
        var documentId = claims.Department != null ? claims.Department.Id : Guid.Empty;
        Document entity = new Document(claims.Id, payload.Title, driveFile.WebContentLink, payload.IsActive, payload.DriveDocId, payload.Description, documentId, claims.Organization.Id);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.Procedure = proc;
        entity.Status = DocumentStatus.PROCESSING;
        entity.CreatedBy = claims.Id;

        _dbContext.Documents.Add(entity);
        _dbContext.SaveChanges();

        proc.IsActive = true;
        _dbContext.Procedures.Update(proc);
        _dbContext.SaveChanges();

        var assignIds = await _HandleAssignDocToProcedureSteps(entity, proc.Id);



        //sync document to elastic search
        try
        {
            var filePath = await _storageHelper.DownloadDoc(payload.DriveDocId);
            var content = "";
            using (var docReader = DocLib.Instance.GetDocReader(filePath, new PageDimensions()))
            {
                for (var i = 0; i < docReader.GetPageCount(); i++)
                {
                    using (var pageReader = docReader.GetPageReader(i))
                    {
                        var text = pageReader.GetText();
                        Console.WriteLine(text);
                        content = content + " " + text;
                    }
                }
            }

            System.IO.DirectoryInfo downloadDir = new DirectoryInfo("~/Downloads");
            foreach (FileInfo file in downloadDir.GetFiles())
            {
                if (filePath.Contains(file.Name))
                {
                    file.Delete();
                }
            }

            var esDoc = GetESDoc(entity, claims);
            esDoc.Content = content;
            esDoc.AssignIds = assignIds;
            await _elasticSearchHelper.CreateDoc<EsDocument>(esDoc, ElasticSearchConstants.DOCUMENT_INDEX);

        }
        catch (Exception ex)
        {
            Console.WriteLine("Error when sync doc to elastic search");
        }

        return true;
    }


    public async Task<List<DocumentDto>> GetAll(UserClaims claims, GetDocumentsRequest query)
    {
        if (claims.Role != null && claims.Role.Id.ToString() != SysRole.Admin)
        {
            return new List<DocumentDto>();
        }
        return await FormatDocuments(query, claims);
    }

    public async Task<List<DocumentDto>> GetOrgDocs(UserClaims claims, GetDocumentsRequest query)
    {
        if (claims.Organization != null && claims.Organization.Id.ToString().IsNullOrEmpty())
        {
            return new List<DocumentDto>();
        }

        query.OrgId = claims.Organization.Id.ToString();

        return await FormatDocuments(query, claims);
    }

    public async Task<List<DocumentDto>> GetDepartmentDocs(UserClaims claims, GetDocumentsRequest query)
    {
        if (claims.Department != null && claims.Department.Id.ToString().IsNullOrEmpty())
        {
            return new List<DocumentDto>();
        }

        query.OrgId = claims.Organization.Id.ToString();
        query.DepartmentId = claims.Department.Id.ToString();

        return await FormatDocuments(query, claims);
    }

    public async Task<List<DocumentDto>> GetUserDocs(UserClaims claims, GetDocumentsRequest query)
    {
        if (claims.Id.ToString().IsNullOrEmpty())
        {
            return new List<DocumentDto>();
        }

        query.OrgId = claims.Organization.Id.ToString();
        query.DepartmentId = claims.Department.Id.ToString();
        query.CreatedBy = claims.Id.ToString();

        return await FormatDocuments(query, claims);
    }

    public async Task<List<AssignDocumentDto>> GetAssignedDocs(UserClaims claims, GetDocumentsRequest query)
    {
        try
        {
            var result = new List<AssignDocumentDto>();
            if (claims.Id.ToString().IsNullOrEmpty())
            {
                return result;
            }

            query.AssignId = claims.Id.ToString();
            List<EsDocument> esDocuments = await SearchDocuments(query, ElasticSearchConstants.DOCUMENT_INDEX, claims);
            List<string> documentIds = esDocuments.Select(doc => doc.Id).ToList();

            if (documentIds.Count() == 0 || documentIds == null)
            {
                return result;
            }

            List<Document> docs = _dbContext.Documents.Where(doc => documentIds.Contains(doc.Id.ToString())).ToList();
            List<DocumentDto> docDtos = _mapper.Map<List<DocumentDto>>(docs);
            List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Include(dps => dps.ProcedureStep).Where(dps => documentIds.Contains(dps.Document.Id.ToString())).ToList();

            docSteps.ForEach(ds =>
            {
                var document = docDtos.FirstOrDefault(doc => doc.Id == ds.Document.Id);
                ds = new DocumentProcedureStep(ds);
                if (document != null)
                {

                    var documentStepDto = _mapper.Map<AssignDocumentDto>(document);
                    var step = _mapper.Map<AssignStepDto>(ds.ProcedureStep);
                    step.Status = ds.Status;
                    documentStepDto.Step = step;
                    result.Add(documentStepDto);
                }
            });

            result.ForEach(dto =>
            {
                var esDoc = esDocuments.SingleOrDefault(ed => ed.Id == dto.Id.ToString());
                if (esDoc != null)
                {
                    dto.OrgName = esDoc.OrgName;
                    dto.DepartmentName = esDoc.DepartmentName;
                    dto.ProcedureName = esDoc.ProcedureName;
                    dto.UserFullName = esDoc.UserFullName;
                }
            });

            return result;
        }

        catch (Exception err)
        {
            Console.WriteLine(err);
            return new List<AssignDocumentDto>();
        }

    }

    public async Task<List<DocumentDto>> GetRejectedDocs(UserClaims claims, GetDocumentsRequest query)
    {
        if (claims.Id.ToString().IsNullOrEmpty())
        {
            return new List<DocumentDto>();
        }

        query.RejectedBy = claims.Id.ToString();
        query.UserId = claims.Id.ToString();

        return await FormatDocuments(query, claims);

    }

    public async Task<DocumentDto> GetUserDoc(string id, UserClaims claims)
    {
        var doc = _dbContext.Documents.Include(d => d.Procedure).SingleOrDefault(d => d.Id == Guid.Parse(id));

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

        try
        {
            _dbContext.Documents.Remove(document);
            _dbContext.SaveChanges();
        }

        catch (Exception err)
        {
            Console.WriteLine(err);
        }

        try
        {
            await _elasticSearchHelper.Delete(id, ElasticSearchConstants.DOCUMENT_INDEX);
        }

        catch (Exception err)
        {
            Console.WriteLine(err);
        }

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
        Procedure updatedProc = null;
        if (payload.ProcedureId != null)
        {
            Procedure proc = _dbContext.Procedures.SingleOrDefault(proc => proc.Id == Guid.Parse(payload.ProcedureId));
            List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Where(dps => dps.Document.Id == doc.Id).ToList();
            if (docSteps.Count() > 0 && !docSteps.All(ds => ds.Status == DocumentStepStatus.PROCESSING))
            {
                throw new Exception("exist_step_status_change_of_document");
            }
            doc.Procedure = proc;
            updatedProc = proc;
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

            doc.Path = driveFile.WebContentLink;
        }

        doc.Title = payload.Title;
        doc.Description = payload.Description;
        doc.UpdatedBy = claims.Id;


        _dbContext.Documents.Update(doc);
        _dbContext.SaveChanges();

        if (updatedProc != null)
        {
            updatedProc.IsActive = true;
            _dbContext.Procedures.Update(updatedProc);
            _dbContext.SaveChanges();
        }

        var assignIds = await _HandleAssignDocToProcedureSteps(doc, doc.Procedure.Id);

        try
        {
            var esDoc = GetESDoc(doc, claims);
            esDoc.AssignIds = assignIds;
            await _elasticSearchHelper.UpdateDoc<EsDocument>(esDoc, id, ElasticSearchConstants.DOCUMENT_INDEX);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error when update doc to elastic search");
        }
        return true;
    }

    public async Task<bool> ApproveDocStep(ApproveDocumentRequest payload, string id, UserClaims claims)
    {
        DocumentProcedureStep docStep = _dbContext.DocumentProcedureSteps.Include(dps => dps.ProcedureStep).SingleOrDefault((dps) => dps.ProcedureStep.Id == Guid.Parse(payload.ProcedureStepId) && dps.Document.Id == Guid.Parse(id));
        if (docStep == null)
        {
            throw new Exception("doc_step_is_not_found");
        }

        ProcedureStep step = _dbContext.ProcedureSteps.SingleOrDefault(ps => ps.Id == docStep.ProcedureStep.Id);

        if (step.AssignId != claims.Id)
        {
            throw new Exception("user_not_assigned_this_step");
        }


        if (payload.IsSign)
        {
            //sign document;
            var signedDocId = await SignDoc(id, payload.SignaturePassword, claims, step.Priority);
            var driveDoc = await _storageHelper.GetFile(signedDocId);

            docStep.DocSignedPath = driveDoc.WebContentLink;
            docStep.DocSignedId = driveDoc.Id;
            docStep.IsSigned = true;
        }
        else
        {
            docStep.IsSigned = false;
        }


        docStep.Status = DocumentStepStatus.APPROVED;
        _dbContext.DocumentProcedureSteps.Update(docStep);
        _dbContext.SaveChanges();

        //check if all doc steps is approved => change document to approved
        List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Where((dps) => dps.Document.Id == Guid.Parse(id)).ToList();
        bool isAllApproved = docSteps.All(ds => ds.Status == DocumentStepStatus.APPROVED);

        if (isAllApproved)
        {
            Document doc = _dbContext.Documents.Include(d => d.Procedure).SingleOrDefault(d => d.Id == Guid.Parse(id));
            doc.Status = DocumentStatus.PUBLISHED;
            _dbContext.Documents.Update(doc);
            _dbContext.SaveChanges();
            try
            {
                var esDoc = await _elasticSearchHelper.GetDoc<EsDocument>(id, ElasticSearchConstants.DOCUMENT_INDEX);
                esDoc.Status = doc.Status;
                await _elasticSearchHelper.UpdateDoc<EsDocument>(esDoc, id, ElasticSearchConstants.DOCUMENT_INDEX);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when update doc to elastic search");
            }
        }


        return true;
    }

    private async Task<string> SignDoc(string docId, string pwd, UserClaims claims, int priority)
    {
        try
        {
            //get data
            var user = _dbContext.Accounts.Include(a => a.Department).Where(a => a.Id == claims.Id).SingleOrDefault();
            var sign = _dbContext.DigitalSignature.Where(ds => ds.User.Id == user.Id && ds.IsDefault == true).FirstOrDefault();
            var doc = _dbContext.Documents.SingleOrDefault(d => d.Id == Guid.Parse(docId));

            if (string.IsNullOrEmpty(doc.DriveDocId) || string.IsNullOrWhiteSpace(doc.DriveDocId))
            {
                throw new Exception("drive_doc_is_not_exist");
            }

            //sign
            var filePath = await _storageHelper.DownloadDoc(doc.DriveDocId);
            var cert = await _storageHelper.DownloadCert(sign.FileId);
            var signedDoc = await _digitalSignHelper.SignDocument(filePath, cert, doc.Title, pwd, sign.Name, sign.HashPassword, $"{user.FirstName} {user.LastName}", priority);

            //save signed file
            Organization org = new Organization();
            Department dep = new Department();
            string driveFolderId = "";
            string fileId = "";

            if (user.OrgId != null)
            {
                org = _dbContext.Organizations.SingleOrDefault(o => o.Id == Guid.Parse(user.OrgId));
            }
            if (user.Department != null)
            {
                dep = _dbContext.Departments.SingleOrDefault(d => d.Id == user.Department.Id);
            }

            using (FileStream stream = new FileStream(Path.Combine(signedDoc.Path), FileMode.Open))
            {

                if (org != null && org.OrgDriveFolderId != null && org.OrgDriveFolderId.Count() > 0) driveFolderId = org.OrgDriveFolderId;
                if (dep != null && dep.DepartmentDriveFolderId != null && dep.DepartmentDriveFolderId.Count() > 0) driveFolderId = dep.DepartmentDriveFolderId;

                string fileMime = MimeMapping.MimeUtility.GetMimeMapping(signedDoc.Name);
                fileId = await _storageHelper.UploadFile(stream, $"Approved_{signedDoc.Name}", fileMime, driveFolderId);
            }

            System.IO.DirectoryInfo signDir = new DirectoryInfo("~/SignedDocs");

            foreach (FileInfo file in signDir.GetFiles())
            {
                if (signedDoc.Path.Contains(file.Name))
                {
                    file.Delete();
                }
            }

            System.IO.DirectoryInfo downloadDir = new DirectoryInfo("~/Downloads");
            foreach (FileInfo file in downloadDir.GetFiles())
            {
                if (filePath.Contains(file.Name))
                {
                    file.Delete();
                }
            }

            return fileId;

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<bool> RejectDocStep(RejectDocumentRequest payload, string id, UserClaims claims)
    {
        DocumentProcedureStep docStep = _dbContext.DocumentProcedureSteps.Include(dps => dps.ProcedureStep).SingleOrDefault((dps) => dps.ProcedureStep.Id == Guid.Parse(payload.ProcedureStepId) && dps.Document.Id == Guid.Parse(id));
        if (docStep == null)
        {
            throw new Exception("doc_step_is_not_found");
        }

        ProcedureStep step = _dbContext.ProcedureSteps.SingleOrDefault(ps => ps.Id == docStep.ProcedureStep.Id);

        if (step.AssignId != claims.Id)
        {
            throw new Exception("user_not_assigned_this_step");
        }

        docStep.Status = DocumentStepStatus.REJECTED;
        _dbContext.DocumentProcedureSteps.Update(docStep);
        _dbContext.SaveChanges();

        List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Where((dps) => dps.Document.Id == Guid.Parse(id) && dps.Status == DocumentStepStatus.PROCESSING).ToList();
        docSteps.ForEach(docStep =>
        {

            docStep.Status = DocumentStepStatus.REJECTED;
            _dbContext.DocumentProcedureSteps.Update(docStep);

        });

        _dbContext.SaveChanges();
        Document doc = _dbContext.Documents.Include(d => d.Procedure).SingleOrDefault(d => d.Id == Guid.Parse(id));
        doc.Status = DocumentStatus.REJECTED;
        _dbContext.Documents.Update(doc);
        _dbContext.SaveChanges();

        try
        {
            var esDoc = await _elasticSearchHelper.GetDoc<EsDocument>(id, ElasticSearchConstants.DOCUMENT_INDEX);
            esDoc.RejectedBy = claims.Id.ToString();
            esDoc.Status = doc.Status;
            await _elasticSearchHelper.UpdateDoc<EsDocument>(esDoc, id, ElasticSearchConstants.DOCUMENT_INDEX);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error when update doc to elastic search");
        }

        return true;
    }

    public async Task<List<EsDocument>> SearchDocuments(GetDocumentsRequest payload, string index, UserClaims claims)
    {
        var esDocClient = _elasticSearchHelper.GetNESTClient(index);
        var boolQuery = new Nest.BoolQuery();
        var mustQueries = new List<QueryContainer>();

        if (IsExistStringFilter(payload.Title))
        {
            mustQueries.Add(new Nest.MatchQuery
            {
                Field = "title",
                Query = payload.Title
            });
        }

        if (IsExistStringFilter(payload.UserFullName))
        {
            mustQueries.Add(new Nest.MatchQuery
            {
                Field = "userFullName",
                Query = payload.UserFullName
            });
        }

        if (IsExistStringFilter(payload.OrgName))
        {
            mustQueries.Add(new Nest.MatchQuery
            {
                Field = "orgName",
                Query = payload.OrgName
            });
        }

        if (IsExistStringFilter(payload.DepartmentName))
        {
            mustQueries.Add(new Nest.MatchQuery
            {
                Field = "departmentName",
                Query = payload.DepartmentName
            });
        }

        if (IsExistStringFilter(payload.ProcedureName))
        {
            mustQueries.Add(new Nest.MatchQuery
            {
                Field = "procedureName",
                Query = payload.ProcedureName
            });
        }

        if (IsExistStringFilter(payload.Status))
        {
            mustQueries.Add(new Nest.MatchPhraseQuery
            {
                Field = "status",
                Query = payload.Status
            });
        }

        if (IsExistStringFilter(payload.CreatedBy))
        {
            mustQueries.Add(new Nest.MatchPhraseQuery
            {
                Field = "createdBy",
                Query = payload.CreatedBy
            });
        }

        if (IsExistStringFilter(payload.OrgId))
        {
            mustQueries.Add(new Nest.MatchPhraseQuery
            {
                Field = "orgId",
                Query = payload.OrgId
            });
        }

        if (IsExistStringFilter(payload.DepartmentId))
        {
            mustQueries.Add(new Nest.MatchPhraseQuery
            {
                Field = "departmentId",
                Query = payload.DepartmentId
            });
        }

        if (IsExistStringFilter(payload.UserId))
        {
            mustQueries.Add(new Nest.MatchPhraseQuery
            {
                Field = "userId",
                Query = payload.UserId
            });
        }

        if (IsExistStringFilter(payload.AssignId))
        {
            //mustQueries.Add(new Nest.NestedQuery
            //{
            //    Path = "assignIds",
            //    Query = new Nest.MatchQuery
            //    {
            //        Field = "assignIds.element",
            //        Query = payload.AssignId
            //    }
            //});

            mustQueries.Add(new Nest.TermQuery
            {
                Field = "assignIds",
                Value = payload.AssignId
            });
        }

        if (IsExistStringFilter(payload.RejectedBy))
        {
            mustQueries.Add(new Nest.MatchPhraseQuery
            {
                Field = "rejectedBy",
                Query = payload.RejectedBy
            });
        }

        var searchRequest = new SearchRequest
        {
            Query = boolQuery
        };

        var response = esDocClient.Search<EsDocument>(s => s
            .Query(q => q
                .Bool(b => b
                    .Must(mustQueries.ToArray()) // Convert the list to an array
                )
            ));

        if (response.IsValid)
        {
            return response.Documents.ToList();
        }

        return new List<EsDocument>();
    }


    public async Task<List<DocumentDto>> SearchPublishDocs(SearchDocumentRequest query)
    {
        var esDocClient = _elasticSearchHelper.GetNESTClient(ElasticSearchConstants.DOCUMENT_INDEX);
        var boolQuery = new Nest.BoolQuery();
        var mustQueries = new List<QueryContainer>();

        if (IsExistStringFilter(query.Filter))
        {
            mustQueries.Add(new Nest.MatchQuery
            {
                Field = "content",
                Query = query.Filter
            });
        }

        if (IsExistStringFilter(query.Title))
        {
            mustQueries.Add(new Nest.MatchQuery
            {
                Field = "title",
                Query = query.Title
            });
        }

        if (IsExistStringFilter(query.Title))
        {
            mustQueries.Add(new Nest.MatchPhraseQuery
            {
                Field = "status",
                Query = DocumentStatus.PUBLISHED
            });
        }


        var searchRequest = new SearchRequest
        {
            Query = boolQuery
        };

        var response = esDocClient.Search<EsDocument>(s => s
            .Query(q => q
                .Bool(b => b
                    .Must(mustQueries.ToArray()) // Convert the list to an array
                )
            ));

        var esDocuments = new List<EsDocument>();
        if (response.IsValid)
        {
            esDocuments = response.Documents.ToList();
        }

        List<string> documentIds = esDocuments.Select(doc => doc.Id).ToList();

        if (documentIds.Count() == 0 || documentIds == null)
        {
            return new List<DocumentDto>();
        }

        var docs = _dbContext.Documents.Where(doc => documentIds.Any(id => id == doc.Id.ToString())).ToList();

        var dto = _mapper.Map<List<DocumentDto>>(docs);

        dto.ForEach(dto =>
        {
            var esDoc = esDocuments.SingleOrDefault(ed => ed.Id == dto.Id.ToString());
            if (esDoc != null)
            {
                dto.OrgName = esDoc.OrgName;
                dto.DepartmentName = esDoc.DepartmentName;
                dto.ProcedureName = esDoc.ProcedureName;
                dto.UserFullName = esDoc.UserFullName;
            }
        });
        return dto;
    }

    private async Task<List<DocumentDto>> FormatDocuments(GetDocumentsRequest query, UserClaims claims)
    {

        List<EsDocument> esDocuments = await SearchDocuments(query, ElasticSearchConstants.DOCUMENT_INDEX, claims);
        List<string> documentIds = esDocuments.Select(doc => doc.Id).ToList();

        if (documentIds.Count() == 0 || documentIds == null)
        {
            return new List<DocumentDto>();
        }

        var docs = _dbContext.Documents.Where(doc => documentIds.Any(id => id == doc.Id.ToString())).ToList();

        var dto = _mapper.Map<List<DocumentDto>>(docs);

        dto.ForEach(dto =>
        {
            var esDoc = esDocuments.SingleOrDefault(ed => ed.Id == dto.Id.ToString());
            if (esDoc != null)
            {
                dto.OrgName = esDoc.OrgName;
                dto.DepartmentName = esDoc.DepartmentName;
                dto.ProcedureName = esDoc.ProcedureName;
                dto.UserFullName = esDoc.UserFullName;
            }
        });
        return dto;
    }

    //pe-condition: all procedure steps of document must be processing
    private async Task<string> _HandleAssignDocToProcedureSteps(Document doc, Guid procedureId)
    {
        //remove old steps
        List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Where(ds => ds.Document.Id == doc.Id).ToList();

        docSteps.ForEach(ds =>
        {
            _dbContext.DocumentProcedureSteps.Remove(ds);
        });

        _dbContext.SaveChanges();

        var assignIds = "";
        //add new steps
        List<ProcedureStep> steps = _dbContext.ProcedureSteps.Where(ps => ps.Procedure.Id == procedureId).ToList();
        steps.ForEach(step =>
        {
            var dps = new DocumentProcedureStep(procedureId, DocumentStepStatus.PROCESSING);
            dps.Document = doc;
            dps.ProcedureStep = step;
            assignIds = assignIds == "" ? $"{step.AssignId}" : $"{assignIds},{step.AssignId}";
            _dbContext.DocumentProcedureSteps.Add(dps);
        });

        _dbContext.SaveChanges();
        return assignIds;
    }

    private EsDocument GetESDoc(Document entity, UserClaims claims)
    {

        return new EsDocument
        {
            Id = entity.Id.ToString(),
            Title = entity.Title,
            CreatedBy = claims.Id.ToString(),
            CreatedAt = DateTime.Now,
            DepartmentId = claims.Department != null ? claims.Department.Id.ToString() : string.Empty,
            DepartmentName = claims.Department != null ? claims.Department.Name : string.Empty,
            DriveDocId = entity.DriveDocId,
            OrgId = claims.Organization != null ? claims.Organization.Id.ToString() : string.Empty,
            OrgName = claims.Organization != null ? claims.Organization.Name : string.Empty,
            ProcedureId = entity.Procedure.Id.ToString(),
            ProcedureName = entity.Procedure.Name,
            Path = entity.Path,
            UserFullName = $"{claims.FirstName} {claims.LastName}",
            UserId = claims.Id.ToString(),
            Status = entity.Status,
            // RejectedBy = entity.RejectedBy
        };
    }

    private bool IsExistStringFilter(string value)
    {
        return !value.IsNullOrEmpty() && value.Count() > 0;
    }

    public async Task<bool> VerifyDocSignature(VerifyDocumentSignatureRequest req, UserClaims claims)
    {
        try
        {
            var docStep = _dbContext.DocumentProcedureSteps.SingleOrDefault(dps => dps.Document.Id == Guid.Parse(req.DocId) && dps.ProcedureStep.Id == Guid.Parse(req.ProcedureStepId));
            if (docStep != null && !docStep.IsSigned)
            {
                throw new Exception("document_was_not_signed");
            }

            var filePath = await _storageHelper.DownloadDoc(docStep.DocSignedId);

            var signature = await _digitalSignHelper.GetSignature(filePath);
            var isValid = _digitalSignHelper.VerifySignature(signature);

            System.IO.DirectoryInfo downloadDir = new DirectoryInfo("~/Downloads");

            foreach (FileInfo file in downloadDir.GetFiles())
            {
                if (filePath.Contains(file.Name))
                {
                    file.Delete();
                }
            }

            return isValid;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<List<DocStepDto>> GetDocSteps(string id, UserClaims claims)
    {
        try
        {
            var docSteps = _dbContext.DocumentProcedureSteps.Include(dps => dps.Document).Include(dps => dps.ProcedureStep).Where(dps => dps.Document.Id == Guid.Parse(id));

            if (docSteps.Count() < 0)
            {
                return new List<DocStepDto>();
            }
            var result = _mapper.Map<List<DocStepDto>>(docSteps);

            result.ForEach(r =>
            {
                var docStep = docSteps.FirstOrDefault(ds => ds.Id == Guid.Parse(r.Id));
                r.Priority = docStep.ProcedureStep.Priority;

                var user = _dbContext.Accounts.SingleOrDefault(a => a.Id == docStep.ProcedureStep.AssignId);
                r.Assigner = $"{user.FirstName} {user.LastName}";
            });

            return result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}


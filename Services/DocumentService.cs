﻿using AutoMapper;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApi.Common.Constants;
using WebApi.Entities;
using WebApi.Helpers;
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
    public DocumentService(DataContext dbContext, IMapper mapper, StorageHelper storageHelper, ElasticSearchHelper elasticSearchHelper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _storageHelper = storageHelper;
        _elasticSearchHelper = elasticSearchHelper;
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
            var esDoc = GetESDoc(entity, claims);
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
                    documentStepDto.Step = step;
                    result.Add(documentStepDto);
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


        docStep.Status = DocumentStepStatus.APPROVED;
        _dbContext.DocumentProcedureSteps.Update(docStep);
        _dbContext.SaveChanges();

        //check if all doc steps is approved => change document to approved
        List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Where((dps) => dps.Document.Id == Guid.Parse(id)).ToList();
        bool isAllApproved = docSteps.All(ds => ds.Status == DocumentStepStatus.APPROVED);

        if (isAllApproved)
        {
            Document doc = _dbContext.Documents.SingleOrDefault(d => d.Id == Guid.Parse(id));
            doc.Status = DocumentStatus.PUBLISHED;
            _dbContext.Documents.Update(doc);
            _dbContext.SaveChanges();
            try
            {
                var esDoc = GetESDoc(doc, claims);
                await _elasticSearchHelper.UpdateDoc<EsDocument>(esDoc, id, ElasticSearchConstants.DOCUMENT_INDEX);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when update doc to elastic search");
            }
        }


        return true;
    }

    public async Task<bool> RejectDocStep(RejectDocumentRequest payload, string id, UserClaims claims)
    {
        DocumentProcedureStep docStep = _dbContext.DocumentProcedureSteps.SingleOrDefault((dps) => dps.Id == Guid.Parse(payload.ProcedureStepId) && dps.Document.Id == Guid.Parse(id));
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
        Document doc = _dbContext.Documents.SingleOrDefault(d => d.Id == Guid.Parse(id));
        doc.Status = DocumentStatus.REJECTED;
        _dbContext.Documents.Update(doc);
        _dbContext.SaveChanges();

        try
        {
            var esDoc = GetESDoc(doc, claims);
            esDoc.RejectedBy = claims.Id.ToString();
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

        QueryDescriptor<EsDocument> conditionQuery = new QueryDescriptor<EsDocument>();

        if (IsExistStringFilter(payload.Title))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.Title).Query(payload.Title)
                                           );
        }

        if (IsExistStringFilter(payload.UserFullName))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.UserFullName).Query(payload.UserFullName)
                                           );
        }

        if (IsExistStringFilter(payload.OrgName))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.OrgName).Query(payload.OrgName)
                                           );
        }

        if (IsExistStringFilter(payload.DepartmentName))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.DepartmentName).Query(payload.DepartmentName)
                                           );
        }

        if (IsExistStringFilter(payload.ProcedureName))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.ProcedureName).Query(payload.ProcedureName)
                                           );
        }

        if (IsExistStringFilter(payload.Status))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.Status).Query(payload.Status)
                                           );
        }

        if (IsExistStringFilter(payload.CreatedBy))
        {
            conditionQuery = conditionQuery.Term(term => term.CreatedBy, payload.CreatedBy);
        }

        if (IsExistStringFilter(payload.OrgId))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.OrgId).Query(payload.OrgId)
                                           );
        }

        if (IsExistStringFilter(payload.DepartmentId))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.DepartmentId).Query(payload.DepartmentId)
                                           );
        }

        if (IsExistStringFilter(payload.UserId))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.UserId).Query(payload.UserId)
                                           );
        }

        if (IsExistStringFilter(payload.AssignId))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.AssignIds).Query(payload.AssignId)
                                           );
        }

        if (IsExistStringFilter(payload.RejectedBy))
        {
            conditionQuery = conditionQuery.Match(mat =>
                                            mat.Field(f => f.RejectedBy).Query(payload.RejectedBy)
                                           );
        }

        var response = await _elasticSearchHelper.Client.SearchAsync<EsDocument>(
                   es =>
                   {
                       es.Index(index);
                       es.Query(q =>
                               q.Bool(b =>
                                   b.Must(conditionQuery)
                                )
                          );
                   });

        if (response.IsValidResponse)
        {
            return response.Documents.ToList();
        }

        return new List<EsDocument>();
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
    private async Task<List<string>> _HandleAssignDocToProcedureSteps(Document doc, Guid procedureId)
    {
        //remove old steps
        List<DocumentProcedureStep> docSteps = _dbContext.DocumentProcedureSteps.Where(ds => ds.Document.Id == doc.Id).ToList();

        docSteps.ForEach(ds =>
        {
            _dbContext.DocumentProcedureSteps.Remove(ds);
        });

        _dbContext.SaveChanges();

        var assignIds = new List<string>();
        //add new steps
        List<ProcedureStep> steps = _dbContext.ProcedureSteps.Where(ps => ps.Procedure.Id == procedureId).ToList();
        steps.ForEach(step =>
        {
            var dps = new DocumentProcedureStep(procedureId, DocumentStepStatus.PROCESSING);
            dps.Document = doc;
            dps.ProcedureStep = step;
            assignIds.Add(step.AssignId.ToString());
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
            Status = entity.Status
        };
    }

    private bool IsExistStringFilter(string value)
    {
        return !value.IsNullOrEmpty() && value.Count() > 0;
    }

}
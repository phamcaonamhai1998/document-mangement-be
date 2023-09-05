using Microsoft.AspNetCore.Mvc;
using WebApi.Helpers;
using WebApi.Models.Documents;
using WebApi.Services.Interfaces;
using WebApi.Authorization;
using WebApi.Common.Constants;
using Org.BouncyCastle.Ocsp;
using WebApi.Models.Users;
using WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentController : BaseController
{
    DataContext _dbContext;
    StorageHelper _storageHelper;
    ElasticSearchHelper _esHelper;
    IWebHostEnvironment _hostingEnvironment;
    IDocumentService _documentService;
    DigitalSignHelper _digitalSignHelper;

    public DocumentController(DataContext dbContext, StorageHelper storageHelper, IWebHostEnvironment hostingEnvironment, IDocumentService documentService, ElasticSearchHelper esHelper, DigitalSignHelper digitalSignHelper)
    {
        _dbContext = dbContext;
        _storageHelper = storageHelper;
        _hostingEnvironment = hostingEnvironment;
        _documentService = documentService;
        _esHelper = esHelper;
        _digitalSignHelper = digitalSignHelper;
    }

    [HttpGet]
    [AuthorizeAttribute("Document:List")]
    public async Task<List<DocumentDto>> GetDocs([FromQuery] GetDocumentsRequest query)
    {
        List<DocumentDto> result;
        switch (Claims.Role.Id.ToString())
        {
            case RoleConstants.ADMIN_ROLE_ID:
                result = await _documentService.GetAll(Claims, query);
                return result;
            case RoleConstants.ORG_OWNER_ID:
                result = await _documentService.GetOrgDocs(Claims, query);
                return result;
            case RoleConstants.DEP_OWNER_ID:
                result = await _documentService.GetDepartmentDocs(Claims, query);
                return result;
            default:
                query.CreatedBy = Claims.Id.ToString();
                result = await _documentService.GetUserDocs(Claims, query);
                return result;
        }

    }


    [HttpGet("search")]
    [Authorize]
    public async Task<List<DocumentDto>> SearchPublishDocs([FromQuery] SearchDocumentRequest query)
    {
        List<DocumentDto> result = new List<DocumentDto>();
        result = await _documentService.SearchPublishDocs(query);
        return result;

    }


    [HttpGet("assigned")]
    [AuthorizeAttribute("Document:List")]
    public async Task<List<AssignDocumentDto>> GetAssignedDocs([FromQuery] GetDocumentsRequest query)
    {
        var result = await _documentService.GetAssignedDocs(Claims, query);
        return result;
    }

    [HttpGet("rejected")]
    [AuthorizeAttribute("Document:List")]
    public async Task<List<DocumentDto>> GetRejectedDocs([FromQuery] GetDocumentsRequest query)
    {
        var result = await _documentService.GetRejectedDocs(Claims, query);
        return result;
    }

    [HttpGet("{id}")]
    [AuthorizeAttribute("Document:List")]
    public async Task<DocumentDto> GetUserDoc(string id)
    {
        var result = await _documentService.GetUserDoc(id, Claims);
        return result;
    }

    [HttpPost("upload/{id}")]
    [RequestFormLimits(MultipartBoundaryLengthLimit = 104857600)]
    // [AuthorizeAttribute("Document:Create")]
    public async Task<string> UploadFile([FromForm] IFormFile file, String id)
    {
        string path = Path.Combine("~/", "Uploads");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string fileName = Path.GetFileName(file.Name);
        string fileId = "";
        Account user = _dbContext.Accounts.Include(a => a.Department).SingleOrDefault(a => a.Id == Guid.Parse(id));
        Organization org = new Organization();
        Department dep = new Department();
        if (user.OrgId != null)
        {
            org = _dbContext.Organizations.SingleOrDefault(a => a.Id == Guid.Parse(user.OrgId));
        }
        if (user.Department != null)
        {
            dep = _dbContext.Departments.SingleOrDefault(a => a.Id == user.Department.Id);
        }

        using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
        {
            file.CopyTo(stream);
            string fileMime = MimeMapping.MimeUtility.GetMimeMapping(file.FileName);
            string driveFolderId = null;

            if (org != null && org.OrgDriveFolderId != null && org.OrgDriveFolderId.Count() > 0) driveFolderId = org.OrgDriveFolderId;
            if (dep != null && dep.DepartmentDriveFolderId != null && dep.DepartmentDriveFolderId.Count() > 0) driveFolderId = dep.DepartmentDriveFolderId;
            fileId = await _storageHelper.UploadFile(stream, file.Name, fileMime, driveFolderId);
        }
        return fileId;
    }

    [HttpPost]
    [AuthorizeAttribute("Document:Create")]
    public async Task<bool> Create([FromBody] CreateDocumentRequest req)
    {
        var result = await _documentService.Create(req, Claims);
        return result;
    }

    [HttpPut("approve/{id}")]
    [AuthorizeAttribute("Document:Approve")]
    public async Task<bool> ApproveDocStep([FromBody] ApproveDocumentRequest req, string id)
    {
        var result = await _documentService.ApproveDocStep(req, id, Claims);
        return result;
    }

    [HttpPut("reject/{id}")]
    [AuthorizeAttribute("Document:Approve")]
    public async Task<bool> RejectDocStep([FromBody] RejectDocumentRequest req, string id)
    {
        var result = await _documentService.RejectDocStep(req, id, Claims);
        return result;
    }

    [HttpPut("{id}")]
    [AuthorizeAttribute("Document:Update")]
    public async Task<bool> UpdateDocProcedure([FromBody] UpdateDocProcedure req, string id)
    {
        var result = await _documentService.UpdateDoc(req, id, Claims);
        return result;
    }
    
    [HttpDelete("{id}")]
    [AuthorizeAttribute("Document:Delete")]
    public async Task<bool> Delete(string id)
    {
        var result = await _documentService.Delete(id, Claims);
        return result;
    }

    
}

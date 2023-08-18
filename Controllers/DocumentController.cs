using Microsoft.AspNetCore.Mvc;
using WebApi.Helpers;
using WebApi.Models.Documents;
using WebApi.Services.Interfaces;
using WebApi.Authorization;
using WebApi.Common.Constants;
using Org.BouncyCastle.Ocsp;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentController : BaseController
{
    StorageHelper _storageHelper;
    IWebHostEnvironment _hostingEnvironment;
    IDocumentService _documentService;

    public DocumentController(StorageHelper storageHelper, IWebHostEnvironment hostingEnvironment, IDocumentService documentService)
    {
        _storageHelper = storageHelper;
        _hostingEnvironment = hostingEnvironment;
        _documentService = documentService;
    }

    [HttpGet]
    [AuthorizeAttribute("Document:List")]
    public async Task<List<DocumentDto>> GetDocs()
    {
        List<DocumentDto> result;
        switch (Claims.Role.Id.ToString())
        {
            case RoleConstants.ADMIN_ROLE_ID:
                result = await _documentService.GetAll(Claims);
                return result;
            case RoleConstants.ORG_OWNER_ID:
                result = await _documentService.GetOrgDocs(Claims);
                return result;
            case RoleConstants.DEP_OWNER_ID:
                result = await _documentService.GetDepartmentDocs(Claims);
                return result;
            default:
                return null;
        }

    }

    [HttpGet("{id}")]
    [AuthorizeAttribute("Document:List")]
    public async Task<DocumentDto> GetUserDoc([FromQuery] string id)
    {
        var result = await _documentService.GetUserDoc(id, Claims);
        return result;
    }

    [HttpPost("upload")]
    [RequestFormLimits(MultipartBoundaryLengthLimit = 104857600)]
    public async Task<string> UploadFile([FromForm] IFormFile file)
    {
        string wwwPath = _hostingEnvironment.WebRootPath;
        string path = Path.Combine("/", "Uploads");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string fileName = Path.GetFileName(file.Name);
        string fileId = "";
        using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
        {
            file.CopyTo(stream);
            string fileMime = MimeMapping.MimeUtility.GetMimeMapping(file.FileName);

            fileId = await _storageHelper.UploadFile(stream, file.Name, fileMime);
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

    [HttpPut("{id}/procedure")]
    [AuthorizeAttribute("Document:Update")]
    public async Task<bool> UpdateDocProcedure([FromBody] UpdateDocProcedure req, string id)
    {
        var result = await _documentService.UpdateDoc(req, id, Claims);
        return result;
    }

    [HttpPut("approve/{id}")]
    [AuthorizeAttribute("Document:Update")]
    public async Task<bool> ApproveDocStep([FromBody] ApproveDocumentRequest req, string id)
    {
        var result = await _documentService.ApproveDocStep(req, id, Claims);
        return result;
    }


    [HttpPut("reject/{id}")]
    [AuthorizeAttribute("Document:Update")]
    public async Task<bool> RejectDocStep([FromBody] RejectDocumentRequest req, string id)
    {
        var result = await _documentService.RejectDocStep(req, id, Claims);
        return result;
    }


    [HttpDelete]
    [AuthorizeAttribute("Document:Delete")]
    public async Task<bool> Delete([FromQuery] string id)
    {
        var result = await _documentService.Delete(id, Claims);
        return result;
    }
}

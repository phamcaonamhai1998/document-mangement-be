using Microsoft.AspNetCore.Mvc;
using WebApi.Helpers;
using WebApi.Models.Documents;
using WebApi.Services.Interfaces;
using WebApi.Authorization;

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

    [HttpGet]
    public async Task<Google.Apis.Drive.v3.Data.File> SearchFile([FromQuery] string id)
    {
        var result = await _storageHelper.GetFile(id);
        return result;
    }


    //[HttpPost("download")]
    //public async void Download([FromQuery] string id)
    //{
    //     _storageHelper.Download(id);
    //}
}

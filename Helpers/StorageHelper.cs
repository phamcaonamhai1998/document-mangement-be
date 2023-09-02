using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using WebApi.Entities;

namespace WebApi.Helpers
{

    public class StorageHelper
    {
        private readonly AppSettings _appSettings;

        public StorageHelper(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;

        }

        private async Task<DriveService> GetService()
        {

            try
            {
                var credential = GoogleCredential.FromFile("service_account.json").CreateScoped(DriveService.ScopeConstants.Drive);
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                });
                return service;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> CreateFolder(string parent, string folderName)
        {
            var service = await GetService();
            var driveFolder = new Google.Apis.Drive.v3.Data.File();
            driveFolder.Name = folderName;
            driveFolder.MimeType = "application/vnd.google-apps.folder";
            driveFolder.Parents = new string[] { parent };
            var command = service.Files.Create(driveFolder);
            var file = command.Execute();
            return file.Id;
        }

        public async Task<string> UploadFile(Stream file, string fileName, string fileMime, string driveFolderId)
        {
            DriveService service = await GetService();

            if (string.IsNullOrEmpty(driveFolderId) || string.IsNullOrWhiteSpace(driveFolderId))
            {
                driveFolderId = _appSettings.Google.SharedFolder;
            }

            var driveFile = new Google.Apis.Drive.v3.Data.File();
            driveFile.Name = fileName;
            driveFile.MimeType = fileMime;
            driveFile.Parents = new string[] { driveFolderId };

            var request = service.Files.Create(driveFile, file, fileMime);
            request.Fields = "id";

            var response = request.Upload();
            if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw response.Exception;

            return request.ResponseBody.Id;
        }

        public async Task<Google.Apis.Drive.v3.Data.File> GetFile(string fileId)
        {
            DriveService service = await GetService();
            var command = service.Files.Get(fileId);
            command.Fields = "id, webViewLink, webContentLink";
            var result = command.Execute();
            return result;
        }


        public async Task<bool> DeleteFile(string driveDocId)
        {
            try
            {
                DriveService service = await GetService();

                var request = service.Files.Delete(driveDocId);

                request.Execute();

            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
            return true;
        }

        public async Task<string> DownloadDoc(string fileId, string userId)
        {
            try
            {
                DriveService service = await GetService();
                var request = service.Files.Get(fileId);

                var stream = new MemoryStream();
                request.Download(stream);

                var path = Path.Combine("~/", "Downloads");
                var pathFile = Path.Combine(path, userId);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }


                FileStream file = new FileStream($"{pathFile}.pdf", FileMode.Create, FileAccess.Write);
                stream.WriteTo(file);
                return $"{pathFile}.pdf";
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task<byte[]> DownloadCert(string fileId)
        {
            DriveService service = await GetService();
            var request = service.Files.Get(fileId);

            var stream = new MemoryStream();
            request.Download(stream);
            var buffers = stream.ToArray();
            return buffers;
        }

        public async Task<string> CreateOrgFolder(string orgName)
        {
            var service = await GetService();
            var driveFolder = new Google.Apis.Drive.v3.Data.File();
            driveFolder.Name = orgName;
            driveFolder.MimeType = "application/vnd.google-apps.folder";
            driveFolder.Parents = new string[] { _appSettings.Google.SharedFolder };
            var command = service.Files.Create(driveFolder);
            var file = command.Execute();
            return file.Id;
        }

        public async Task<string> CreateDepFolder(string depName, string orgFolderId)
        {
            var service = await GetService();
            var driveFolder = new Google.Apis.Drive.v3.Data.File();
            driveFolder.Name = depName;
            driveFolder.MimeType = "application/vnd.google-apps.folder";
            driveFolder.Parents = new string[] { orgFolderId };
            var command = service.Files.Create(driveFolder);
            var file = command.Execute();
            return file.Id;
        }

        public async Task<bool> DeleteFolder(string folderId)
        {
            var service = await GetService();
            var command = service.Files.Delete(folderId);
            command.Execute();
            return true;
        }

        public async Task<bool> UpdateFolderName(string folderId, string name)
        {
            var service = await GetService();
            var driveFolder = new Google.Apis.Drive.v3.Data.File();
            driveFolder.Name = name;
            var command = service.Files.Update(driveFolder, folderId);
            command.Execute();
            return true;
        }

        public async Task<string> CreateUserCertFolder(string userId, string fullName)
        {
            var service = await GetService();
            var driveFolder = new Google.Apis.Drive.v3.Data.File();
            driveFolder.Name = $"{fullName} - {userId}";
            driveFolder.MimeType = "application/vnd.google-apps.folder";
            driveFolder.Parents = new string[] { _appSettings.Google.CertsFolder };
            var command = service.Files.Create(driveFolder);
            var file = command.Execute();
            return file.Id;
        }


        public async Task<string> UploadCert(Stream file, string fileName, string folderId)
        {
            DriveService service = await GetService();

            if (string.IsNullOrEmpty(folderId) || string.IsNullOrWhiteSpace(folderId))
            {
                folderId = _appSettings.Google.CertsFolder;
            }

            var driveFile = new Google.Apis.Drive.v3.Data.File();
            driveFile.Name = fileName;
            driveFile.Parents = new string[] { folderId };

            var request = service.Files.Create(driveFile, file, "application/x-pkcs12");
            request.Fields = "id";

            var response = request.Upload();
            if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw response.Exception;

            return request.ResponseBody.Id;
        }

    }
}

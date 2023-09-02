using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Security;
using System.Drawing;
using WebApi.Entities;
using WebApi.Models.DigitalSignature;

namespace WebApi.Helpers;

public class DigitalSignHelper
{
    public async Task<SignedFileResponse> SignDocument(string filePath, byte[] userCert, string docId, string pwd, string signName, string signPwd, string fullName, string userId)
    {
        var isValidPwd = BCrypt.Net.BCrypt.Verify(pwd, signPwd);
        if (!isValidPwd)
        {
            throw new Exception("password_of_signature_is_invalid");
        }

        // create pdf doc
        PdfDocument doc = new PdfDocument();
        doc.LoadFromFile(filePath);

        // load cert
        PdfCertificate cert = new PdfCertificate(userCert, pwd);

        //create signature obj
        PdfSignature signature = new PdfSignature(doc, doc.Pages[doc.Pages.Count - 1], cert, signName);

        RectangleF rectangleF = new RectangleF(doc.Pages[0].ActualSize.Width - 300, doc.Pages[0].ActualSize.Height - 110, 260, 110);
        signature.Bounds = rectangleF;
        signature.Certificated = true;

        //set the graphic mode to image and sign detail
        signature.GraphicsMode = GraphicMode.SignImageAndSignDetail;
        //Set the signature content
        signature.NameLabel = "Signer:";
        signature.Name = fullName;
        signature.DateLabel = "Date:";
        signature.Date = DateTime.Now;

        //signature.SignImageSource = PdfImage.FromFile("/path/to/img/src");
        signature.SignDetailsFont = new PdfTrueTypeFont(new Font("Arial Unicode MS", 12f, FontStyle.Regular));

        signature.DocumentPermissions = PdfCertificationFlags.ForbidChanges | PdfCertificationFlags.AllowFormFill;

        string path = Path.Combine("~/", "SignedDocs");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var fileName = $"{userId}_{docId}.pdf";
        var savePath = Path.Combine(path, fileName);
        doc.SaveToFile(savePath);
        doc.Close();


        var res = new SignedFileResponse();
        res.Name = fileName;
        res.Path = savePath;
        return res;
    }
}

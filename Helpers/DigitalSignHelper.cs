﻿using Spire.Pdf;
using Spire.Pdf.Security;
using Spire.Pdf.Widget;
using WebApi.Models.DigitalSignature;
using System.Drawing;

namespace WebApi.Helpers;

public class DigitalSignHelper
{
    public async Task<SignedFileResponse> SignDocument(string filePath, byte[] userCert, string docName, string pwd, string signName, string signPwd, string fullName, int priority)
    {
        try
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
            //signature.SignDetailsFont = new PdfTrueTypeFont(new Font("Arial Unicode MS", 12f, FontStyle.Regular));

            signature.DocumentPermissions = PdfCertificationFlags.ForbidChanges | PdfCertificationFlags.AllowFormFill;

            string path = Path.Combine("~/", "SignedDocs");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fileName = $"{fullName}_{docName}_{priority}.pdf";
            var savePath = Path.Combine(path, fileName);
            doc.SaveToFile(savePath);
            doc.Close();


            var res = new SignedFileResponse();
            res.Name = fileName;
            res.Path = savePath;
            return res;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw ex;
        }

    }

    public async Task<PdfSignature> GetSignature(string filename)
    {
        try
        {
            List<PdfSignature> signatures = new List<PdfSignature>();

            PdfDocument doc = new PdfDocument(filename);
            PdfFormWidget form = (PdfFormWidget)doc.Form;
            for (int i = 0; i < form.FieldsWidget.Count; ++i)
            {

                PdfSignatureFieldWidget field = form.FieldsWidget[i] as PdfSignatureFieldWidget;
                if (field != null && field.Signature != null)
                {
                    PdfSignature signature = field.Signature;
                    signatures.Add(signature);
                }
            }
            return signatures.Count() > 0 ? signatures[0] : null;
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    public bool VerifySignature(PdfSignature sign)
    {
        try
        {
            return sign.VerifySignature();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}

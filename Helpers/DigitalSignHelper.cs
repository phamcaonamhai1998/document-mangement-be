using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Security;
using System.Drawing;

namespace WebApi.Helpers;

public class DigitalSignHelper
{
    public void SignDocument()
    {
        // create pdf doc
        PdfDocument doc = new PdfDocument();
        doc.LoadFromFile("DocSample/test.pdf");

        // load cert
        PdfCertificate cert = new PdfCertificate("", "pwd");

        //create signature obj
        PdfSignature signature = new PdfSignature(doc, doc.Pages[doc.Pages.Count - 1], cert, "Signature name");

        RectangleF rectangleF = new RectangleF(doc.Pages[0].ActualSize.Width - 260 - 54, 200, 260, 110);
        signature.Bounds = rectangleF;
        signature.Certificated = true;

        //set the graphic mode to image and sign detail
        signature.GraphicsMode = GraphicMode.SignImageAndSignDetail;
        //Set the signature content
        signature.NameLabel = "Signer:";
        signature.Name = "Gary";
        signature.DateLabel = "Date:";
        signature.Date = DateTime.Now;
        signature.DistinguishedNameLabel = "DN:";
        signature.DistinguishedName = signature.Certificate.IssuerName.Name;

        //signature.SignImageSource = PdfImage.FromFile("/path/to/img/src");
        signature.SignDetailsFont = new PdfTrueTypeFont(new Font("Arial Unicode MS", 12f, FontStyle.Regular));

        signature.DocumentPermissions = PdfCertificationFlags.ForbidChanges | PdfCertificationFlags.AllowFormFill;

        doc.SaveToFile("other.pdf");
        doc.Close();

    }
}

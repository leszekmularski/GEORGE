using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;
public class PdfReaderService
{
    public string ReadPdfTable(string filePath)
    {
        using (PdfReader reader = new PdfReader(filePath))
        using (PdfDocument pdfDoc = new PdfDocument(reader))
        {
            StringBuilder text = new StringBuilder();
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var strategy = new SimpleTextExtractionStrategy();
                string pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);
                text.Append(pageContent);
            }
            return text.ToString();
        }
    }
}


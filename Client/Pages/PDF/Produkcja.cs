using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GEORGE.Client.Pages.PDF
{
    public class PdfDrawingViewModel
    {
        public string? Text { get; set; }
        public List<LineData>? Lines { get; set; }
    }

    public class PdfDataParserRys
    {
        public PdfDrawingViewModel ParsePdfDataRysunek(string pdfText)
        {
            var service = new PdfReaderServiceRys();
            var pdfData = service.ReadPdfWithDrawings(pdfText);

            return new PdfDrawingViewModel
            {
                Text = pdfData.Text,
                Lines = pdfData.Lines
            };
        }
    }

}

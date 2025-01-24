namespace GEORGE.Client.Pages.PDF
{
    public class PdfDrawingViewModel
    {
        public string? Text { get; set; }  // Tekst PDF, ogólnie
        public List<LineData>? Lines { get; set; }  // Linie
        public List<TextData>? Texts { get; set; }  // Nowa właściwość do przechowywania tekstów
        public int PageNumber { get; set; } // Dodaj właściwość numeru strony
    }

    public class PdfDataParserRys
    {
        private readonly PdfReaderServiceRys _service;

        // Konstruktor wstrzykujący serwis
        public PdfDataParserRys(PdfReaderServiceRys service)
        {
            _service = service;
        }

        public async Task<PdfDrawingViewModel> ParsePdfDataRysunekAsync(string pdfText)
        {
            var pdfData = await _service.ReadPdfWithDrawingsAsync(pdfText);

            //// Zmienna przechowująca numer strony (domyślnie 1)
            //int nrPage = 1;

            //// Jeżeli `pdfData` zawiera dane o numerze strony, możesz je wyodrębnić
            //if (pdfData.Lines != null && pdfData.Lines.Any())
            //{
            //    // Zakładamy, że numer strony można odczytać z właściwości (przykład: Line.PageNumber)
            //    nrPage = pdfData.Lines.FirstOrDefault()?.PageNumber ?? 1;
            //}
            //else if (pdfData.Texts != null && pdfData.Texts.Any())
            //{
            //    // Alternatywnie, pobierz numer strony z tekstów
            //    nrPage = pdfData.Texts.FirstOrDefault()?.PageNumber ?? 1;
            //}

           // Console.WriteLine($"Numer strony: {nrPage}");

            return new PdfDrawingViewModel
            {
                Text = pdfData.Text,
                Lines = pdfData.Lines,
                Texts = pdfData.Texts, // Dodajemy listę tekstów
                PageNumber = 0    // Możesz dodać tę informację do modelu
            };
        }

    }
}

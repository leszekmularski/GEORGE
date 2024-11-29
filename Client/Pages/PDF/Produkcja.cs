namespace GEORGE.Client.Pages.PDF
{
    public class PdfDrawingViewModel
    {
        public string? Text { get; set; }  // Tekst PDF, ogólnie
        public List<LineData>? Lines { get; set; }  // Linie
        public List<TextData>? Texts { get; set; }  // Nowa właściwość do przechowywania tekstów
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

            return new PdfDrawingViewModel
            {
                Text = pdfData.Text,
                Lines = pdfData.Lines,
                Texts = pdfData.Texts  // Dodajemy listę tekstów
            };
        }
    }
}

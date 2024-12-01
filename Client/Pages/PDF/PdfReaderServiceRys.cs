using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Geom;
using System.Text;

public class PdfReaderServiceRys
{
    private readonly HttpClient _httpClient;

    // Wstrzykiwanie HttpClient przez konstruktor
    public PdfReaderServiceRys(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(string Text, List<LineData> Lines, List<TextData> Texts)> ReadPdfWithDrawingsAsync(string fileUrl)
    {
        try
        {
            var lines = new List<LineData>();
            var text = new StringBuilder();
            var texts = new List<TextData>(); // Lista do przechowywania tekstów

            // Pobierz bajty PDF
            var pdfBytes = await _httpClient.GetByteArrayAsync(fileUrl);

            using (var stream = new MemoryStream(pdfBytes))
            using (var reader = new PdfReader(stream))
            using (var pdfDoc = new PdfDocument(reader))
            {
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var page = pdfDoc.GetPage(i);

                    // Wyciąganie tekstu
                    //ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                    //ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    ITextExtractionStrategy strategy = new GroupedTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);

                    // Użycie GetActualText(), jeśli dostępne
                    text.AppendLine(pageText);

                    // Wyciąganie rysunków i dodatkowych danych tekstowych
                    var drawingStrategy = new MyCustomDrawingStrategy();
                    var processor = new PdfCanvasProcessor(drawingStrategy);
                    processor.ProcessPageContent(page);

                    // Dodawanie zebranych linii i tekstów
                    lines.AddRange(drawingStrategy.Lines);

                    texts.AddRange(drawingStrategy.Texts);
                }
            }

            return (text.ToString(), lines, texts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}, Link: {fileUrl}");
            return (null, null, null);
        }
    }

}


// Custom strategy to extract lines

public class MyCustomDrawingStrategy : IEventListener
{
    public List<LineData> Lines { get; } = new List<LineData>();
    public List<TextData> Texts { get; } = new List<TextData>(); // Lista do przechowywania danych tekstowych


    public void EventOccurred(IEventData data, EventType type)
    {
        if (type == EventType.RENDER_PATH && data is PathRenderInfo pathInfo)
        {
            var path = pathInfo.GetPath();
            var subpaths = path.GetSubpaths();

            // Pobierz grubość linii
            float lineWidth = pathInfo.GetLineWidth();

            // Zmienna do przechowywania DashPattern
            float[]? dashPatternArray = null; // Zmienna na konwersję PdfArray do float[]
            string lineStyle = "solid"; // Domyślnie przyjmujemy linię jako solidną

            // Pobierz GraphicsState
            var graphicsState = pathInfo.GetGraphicsState();

            // Sprawdzamy DashPattern
            if (graphicsState != null)
            {
                var dashPattern = graphicsState.GetDashPattern();
                if (dashPattern != null && dashPattern.Size() > 0)
                {
                    Console.WriteLine($"dashPattern.Size():{dashPattern.Size()}");  

                    bool isDashed = false;

                    // Iteracja po elementach w DashPattern
                    for (int i = 0; i < dashPattern.Size(); i++)
                    {
                        // Pobieramy wartość jako PdfNumber
                        var dashValue = dashPattern.GetAsNumber(i)?.GetValue();

                        // Sprawdzamy, czy wartość większa niż 0
                        if (dashValue.HasValue && dashValue.Value > 0)
                        {
                            isDashed = true;
                            break;
                        }
                    }

                    // Jeśli mamy przerywaną linię, zmieniamy styl
                    if (isDashed)
                    {
                        lineStyle = "dashed";
                    }
                }
            }
            
            var pageHeight = 800;

            foreach (var subpath in subpaths)
            {
                var segments = subpath.GetSegments();
                foreach (var segment in segments)
                {
                    if (segment is iText.Kernel.Geom.Line line)
                    {
                        var points = line.GetBasePoints();

                        float startX = (float)Math.Round(points[0].GetX(), 0);
                        float startY = (float)Math.Round(points[0].GetY(), 0);
                        float endX = (float)Math.Round(points[1].GetX(), 0);
                        float endY = (float)Math.Round(points[1].GetY(), 0);

                        startY = pageHeight - startY;
                        endY = pageHeight - endY;

                        Lines.Add(new LineData
                        {
                            Start = new Vector(startX, startY, 0),
                            End = new Vector(endX, endY, 0),
                            LineWidth = lineWidth,
                            LineStyle = lineStyle,
                            DashPattern = dashPatternArray // Przypisujemy skonwertowaną tablicę float[]
                        });
                    }
                }
            }
        }

        // Obsługuje także tekst
        if (type == EventType.RENDER_TEXT && data is TextRenderInfo textInfo)
        {
            var text = textInfo.GetActualText() ?? textInfo.GetText();

            var textPosition = textInfo.GetBaseline().GetStartPoint();

            float x = (float)Math.Round(textPosition.Get(0), 0);
            float y = (float)Math.Round(textPosition.Get(1), 0);

            // Dostosowanie Y do układu współrzędnych PDF (odwrócenie osi Y)
            y = 800 - y;

            Texts.Add(new TextData
            {
                Text = text,
                Position = new Vector(x, y, 0)
            });
        }
    }



    public ICollection<EventType> GetSupportedEvents() => new List<EventType> { EventType.RENDER_PATH, EventType.RENDER_TEXT };
}

// Line data model
public class LineData
{
    public Vector? Start { get; set; }
    public Vector? End { get; set; }
    public float LineWidth { get; set; } // Grubość linii
    public string? LineStyle { get; set; } // Typ linii: "solid" lub "dashed"
   public float[]? DashPattern { get; set; } // Szczegóły przerywanego wzoru
}

public class TextData
{
    public string? Text { get; set; }
    public Vector? Position { get; set; }
}


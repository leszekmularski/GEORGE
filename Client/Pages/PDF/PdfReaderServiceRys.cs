using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Geom;
using System.Text;
using System;
using System.Net.Http;
using iText.Commons.Utils;
using System.Net.Http;
using System.Runtime.InteropServices.Marshalling;
using iText.Layout.Font;
using System.Text.Unicode;

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
            var texts = new List<TextData>();  // Lista do przechowywania tekstów

            var pdfBytes = await _httpClient.GetByteArrayAsync(fileUrl);

            using (var stream = new MemoryStream(pdfBytes))
            using (var reader = new PdfReader(stream))
            using (var pdfDoc = new PdfDocument(reader))
            {
 
                //for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                //{
                //    var page = pdfDoc.GetPage(i);
                //    var resources = page.GetResources();

                //    // Pobranie mapy czcionek
                //    var fontResources = resources.GetResource(PdfName.Font);

                //    if (fontResources is PdfDictionary fontDict)
                //    {
                //        foreach (var fontEntry in fontDict.EntrySet())
                //        {
                //            var fontObject = fontEntry.Value;
                //            if (fontObject is PdfDictionary fontDefinition)
                //            {
                //                var fontName = fontEntry.Key.ToString();
                //                Console.WriteLine($"Font found: {fontName}");

                //                // Pobieranie szczegółowych informacji o czcionce
                //                if (fontDefinition.ContainsKey(PdfName.BaseFont))
                //                {
                //                    var baseFont = fontDefinition.GetAsName(PdfName.BaseFont);
                //                    Console.WriteLine($"  Base font: {baseFont}");
                //                }

                //                if (fontDefinition.ContainsKey(PdfName.Subtype))
                //                {
                //                    var subtype = fontDefinition.GetAsName(PdfName.Subtype);
                //                    Console.WriteLine($"  Subtype: {subtype}");
                //                }

                //                if (fontDefinition.ContainsKey(PdfName.Encoding))
                //                {
                //                    var encoding = fontDefinition.GetAsName(PdfName.Encoding);
                //                    Console.WriteLine($"  Encoding: {encoding}");
                //                }

                //                if (fontDefinition.ContainsKey(PdfName.ToUnicode))
                //                {
                //                    var toUnicodeStream = fontDefinition.GetAsStream(PdfName.ToUnicode);
                //                    Console.WriteLine("ToUnicode stream found. This might be needed for proper decoding.");
                //                }

                //            }
                //        }
                //    }
                //}



                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var page = pdfDoc.GetPage(i);

                    // Wyciąganie tekstu
                    var strategy = new SimpleTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);

                    Console.WriteLine(pageText);
      
                    text.Append(pageText);

                    // Wyciąganie rysunków
                    var drawingStrategy = new MyCustomDrawingStrategy();
                    var processor = new PdfCanvasProcessor(drawingStrategy);
                    processor.ProcessPageContent(page);

                    lines.AddRange(drawingStrategy.Lines);
                    texts.AddRange(drawingStrategy.Texts);  // Dodajemy zebrane teksty
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
            // Pobierz ścieżkę rysowaną na stronie
            var path = pathInfo.GetPath();
            var subpaths = path.GetSubpaths();

            var pageHeight = 800;

            foreach (var subpath in subpaths)
            {
                var segments = subpath.GetSegments();
                foreach (var segment in segments)
                {
                    if (segment is iText.Kernel.Geom.Line line)
                    {
                        var points = line.GetBasePoints(); // Zwróci listę punktów, z których dwa są interesujące

                        float startX = (float)Math.Round(points[0].GetX(), 0);
                        float startY = (float)Math.Round(points[0].GetY(), 0);
                        float endX = (float)Math.Round(points[1].GetX(), 0);
                        float endY = (float)Math.Round(points[1].GetY(), 0);

                        startY = pageHeight - startY;
                        endY = pageHeight - endY;

                        Lines.Add(new LineData
                        {
                            Start = new Vector(startX, startY, 0), // Punkt początkowy
                            End = new Vector(endX, endY, 0),        // Punkt końcowy
                        });
                    }
                }
            }
        }
        // Obsługuje także tekst
        if (type == EventType.RENDER_TEXT && data is TextRenderInfo textInfo)
        {
            var text = textInfo.GetText();
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
}

public class TextData
{
    public string? Text { get; set; }
    public Vector? Position { get; set; }
}


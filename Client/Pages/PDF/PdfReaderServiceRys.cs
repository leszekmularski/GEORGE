using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Geom;
using System.Text;

public class PdfReaderServiceRys
{
    public (string Text, List<LineData> Lines) ReadPdfWithDrawings(string filePath)
    {
        var lines = new List<LineData>();
        var text = new StringBuilder();

        using (var reader = new PdfReader(filePath))
        using (var pdfDoc = new PdfDocument(reader))
        {
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var page = pdfDoc.GetPage(i);

                // Extract text
                var strategy = new LocationTextExtractionStrategy();
                string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                text.Append(pageText);

                // Extract drawing elements
                var drawingStrategy = new MyCustomDrawingStrategy();
                PdfCanvasProcessor processor = new PdfCanvasProcessor(drawingStrategy);
                processor.ProcessPageContent(page);

                lines.AddRange(drawingStrategy.Lines);
            }
        }

        return (text.ToString(), lines);
    }
}

// Custom strategy to extract lines
public class MyCustomDrawingStrategy : IEventListener
{
    public List<LineData> Lines { get; } = new List<LineData>();

    public void EventOccurred(IEventData data, EventType type)
    {
        if (type == EventType.RENDER_PATH && data is PathRenderInfo pathInfo)
        {
            // Pobierz ścieżkę rysowaną na stronie
            var path = pathInfo.GetPath();
            var subpaths = path.GetSubpaths();

            // Iteracja przez subścieżki i segmenty
            foreach (var subpath in subpaths)
            {
                var segments = subpath.GetSegments();
                foreach (var segment in segments)
                {
                    if (segment is LineSegment line)
                    {
                        Lines.Add(new LineData
                        {
                            Start = line.GetStartPoint(),
                            End = line.GetEndPoint()
                        });
                    }
                }
            }
        }
    }

    public ICollection<EventType> GetSupportedEvents() => null;
}


// Line data model
public class LineData
{
    public Vector? Start { get; set; }
    public Vector? End { get; set; }
}

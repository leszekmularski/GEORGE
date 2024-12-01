using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using System.Text;
using System.Collections.Generic;
using System.Linq;

public class GroupedTextExtractionStrategy : ITextExtractionStrategy
{
    private readonly List<TextChunk> _textChunks = new();

    public void EventOccurred(IEventData data, EventType type)
    {
        if (type == EventType.RENDER_TEXT && data is TextRenderInfo renderInfo)
        {
            // Pobierz tekst i współrzędne
            var text = renderInfo.GetText().Normalize(NormalizationForm.FormC);
            var baseline = renderInfo.GetBaseline().GetStartPoint();
            var x = (float)Math.Round(baseline.Get(0), 2); // Zaokrąglenie do dwóch miejsc po przecinku
            var y = (float)Math.Round(baseline.Get(1), 2);

            // Dodaj do listy fragmentów
            _textChunks.Add(new TextChunk(text, x, y));
        }
    }

    public string GetResultantText()
    {
        // Grupowanie fragmentów w linie na podstawie współrzędnych Y
        var groupedLines = _textChunks
            .GroupBy(chunk => chunk.Y)
            .OrderByDescending(group => group.Key) // W PDF Y rośnie w dół, więc odwrócona kolejność
            .Select(group =>
                string.Join(" ", group.OrderBy(chunk => chunk.X).Select(chunk => chunk.Text))
            );

        // Scal wszystkie linie w jeden wynikowy tekst
        return string.Join("\n", groupedLines);
    }

    public ICollection<EventType> GetSupportedEvents() => new HashSet<EventType> { EventType.RENDER_TEXT };

    private class TextChunk
    {
        public string Text { get; }
        public float X { get; }
        public float Y { get; }

        public TextChunk(string text, float x, float y)
        {
            Text = text;
            X = x;
            Y = y;
        }
    }
}

using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XSquareShape : IShapeDC
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Size { get; set; }
        public string NazwaObj { get; set; } = "Kwadrat";

        private double _scaleFactor = 1.0;

        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        public List<XPoint> Points { get; set; }
        public string ID { get; set; } = Guid.NewGuid().ToString();

        public List<XPoint> GetPoints() => Points;

        // ---------------------------------------------------------
        // 🔥 Konstruktor — generujemy punkty automatycznie
        // ---------------------------------------------------------
        public XSquareShape(double x, double y, double size, double scaleFactor)
        {
            X = x;
            Y = y;
            Size = size;
            _scaleFactor = scaleFactor;

            Points = GeneratePoints();

            Szerokosc = Size;
            Wysokosc = Size;
        }

        // ---------------------------------------------------------
        // 🔥 Generowanie poprawnych narożników kwadratu
        // ---------------------------------------------------------
        private List<XPoint> GeneratePoints()
        {
            return new List<XPoint>
            {
                new XPoint(X, Y),                 // lewy górny
                new XPoint(X + Size, Y),          // prawy górny
                new XPoint(X + Size, Y + Size),   // prawy dolny
                new XPoint(X, Y + Size)           // lewy dolny
            };
        }

        // ---------------------------------------------------------
        // 🔥 Update gdy przeciągnę punkt narożny
        // ---------------------------------------------------------
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 2)
                return;

            Points = newPoints;

            // Ustal lewy górny i prawy dolny
            double minX = newPoints.Min(p => p.X);
            double minY = newPoints.Min(p => p.Y);
            double maxX = newPoints.Max(p => p.X);
            double maxY = newPoints.Max(p => p.Y);

            X = minX;
            Y = minY;

            // Kwadrat musi być równy → rozmiar to średnia
            double width = maxX - minX;
            double height = maxY - minY;
            Size = (width + height) / 2.0;

            Szerokosc = Size;
            Wysokosc = Size;

            Points = GeneratePoints();
        }

        // ---------------------------------------------------------
        public IShapeDC Clone()
        {
            return new XSquareShape(X, Y, Size, _scaleFactor);
        }

        // ---------------------------------------------------------
        public async Task Draw(Canvas2DContext ctx)
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.RectAsync(X, Y, Size, Size);
            await ctx.StrokeAsync();
        }

        // ---------------------------------------------------------
        public List<EditableProperty> GetEditableProperties() => new()
        {
            new("X", () => X, v => { X = v; Points = GeneratePoints(); }, NazwaObj, true),
            new("Y", () => Y, v => { Y = v; Points = GeneratePoints(); }, NazwaObj, true),
            new("Rozmiar", () => Size, v => { Size = v; Szerokosc = v; Wysokosc = v; Points = GeneratePoints(); }, NazwaObj)
        };

        // ---------------------------------------------------------
        public void Scale(double factor)
        {
            Size *= factor;

            Szerokosc = Size;
            Wysokosc = Size;

            Points = GeneratePoints();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;

            Points = GeneratePoints();
        }

        // ---------------------------------------------------------
        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Size, Size, NazwaObj);
        }

        // ---------------------------------------------------------
        public XRectangleShape ToRectangleShape()
        {
            return new XRectangleShape(X, Y, Size, Size, _scaleFactor);
        }

        // ---------------------------------------------------------
        public void Transform(double scale, double offsetX, double offsetY)
        {
            X = X * scale + offsetX;
            Y = Y * scale + offsetY;
            Size *= scale;

            Szerokosc = Size;
            Wysokosc = Size;

            Points = GeneratePoints();
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = X * scaleX + offsetX;
            Y = Y * scaleY + offsetY;

            // Kwadrat wymaga jednolitego skalowania
            Size *= (scaleX + scaleY) / 2.0;

            Szerokosc = Size;
            Wysokosc = Size;

            Points = GeneratePoints();
        }

        // ---------------------------------------------------------
        public List<XPoint> GetCorners() => GeneratePoints();
        public List<XPoint> GetVertices() => GeneratePoints();

        // ---------------------------------------------------------
        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GeneratePoints();
            return new List<(XPoint, XPoint)>
            {
                (v[0], v[1]),
                (v[1], v[2]),
                (v[2], v[3]),
                (v[3], v[0])
            };
        }
    }
}

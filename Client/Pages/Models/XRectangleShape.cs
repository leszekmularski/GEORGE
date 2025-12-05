using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XRectangleShape : IShapeDC
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        public string NazwaObj { get; set; } = "Prostokąt";

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        private double _scaleFactor = 1.0;

        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() =>
            NominalPoints.Select(p => new XPoint(p.X, p.Y)).ToList();

        // ---------------------------------------------------------
        // Konstruktor
        // ---------------------------------------------------------
        public XRectangleShape(double x, double y, double width, double height, double scaleFactor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _scaleFactor = scaleFactor;

            Points = GeneratePoints();
            NominalPoints = Points.Select(p => new XPoint(p.X, p.Y)).ToList();

            Szerokosc = Width;
            Wysokosc = Height;
        }

        // ---------------------------------------------------------
        // Generowanie punktów narożników
        // ---------------------------------------------------------
        private List<XPoint> GeneratePoints()
        {
            return new List<XPoint>
            {
                new XPoint(X, Y),                     // lewy górny
                new XPoint(X + Width, Y),             // prawy górny
                new XPoint(X + Width, Y + Height),    // prawy dolny
                new XPoint(X, Y + Height)             // lewy dolny
            };
        }

        // ---------------------------------------------------------
        // Aktualizacja punktów od użytkownika
        // ---------------------------------------------------------
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count == 0)
                return;

            Points = newPoints;

            double minX = newPoints.Min(p => p.X);
            double minY = newPoints.Min(p => p.Y);
            double maxX = newPoints.Max(p => p.X);
            double maxY = newPoints.Max(p => p.Y);

            X = minX;
            Y = minY;
            Width = maxX - minX;
            Height = maxY - minY;

            Szerokosc = Width;
            Wysokosc = Height;

            Points = GeneratePoints();
        }

        // ---------------------------------------------------------
        // Clone
        // ---------------------------------------------------------
        public IShapeDC Clone()
        {
            var c = new XRectangleShape(X, Y, Width, Height, _scaleFactor);
            c.Points = Points.Select(p => new XPoint(p.X, p.Y)).ToList();
            c.NominalPoints = NominalPoints.Select(p => new XPoint(p.X, p.Y)).ToList();
            return c;
        }

        public XRectangleShape ToRectangleShape()
        {
            return new XRectangleShape(X, Y, Width, Height, _scaleFactor);
        }


        // ---------------------------------------------------------
        // Rysowanie
        // ---------------------------------------------------------
        public async Task Draw(Canvas2DContext ctx)
        {
            Points = GeneratePoints();

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.RectAsync(X, Y, Width, Height);
            await ctx.StrokeAsync();
        }

        // ---------------------------------------------------------
        // Edytowalne właściwości
        // ---------------------------------------------------------
        public List<EditableProperty> GetEditableProperties() => new()
        {
            new("X", () => X, v => { X = v; Points = GeneratePoints(); }, NazwaObj, true),
            new("Y", () => Y, v => { Y = v; Points = GeneratePoints(); }, NazwaObj, true),
            new("Szerokość", () => Width, v => { Width = v; Points = GeneratePoints(); }, NazwaObj),
            new("Wysokość", () => Height, v => { Height = v; Points = GeneratePoints(); }, NazwaObj)
        };

        // ---------------------------------------------------------
        // Transformacje
        // ---------------------------------------------------------
        public void Scale(double factor)
        {
            Width *= factor;
            Height *= factor;

            Szerokosc = Width;
            Wysokosc = Height;

            Points = GeneratePoints();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;

            Points = GeneratePoints();
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            X = X * scale + offsetX;
            Y = Y * scale + offsetY;
            Width *= scale;
            Height *= scale;

            Points = GeneratePoints();
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = X * scaleX + offsetX;
            Y = Y * scaleY + offsetY;
            Width *= scaleX;
            Height *= scaleY;

            Points = GeneratePoints();
        }

        // ---------------------------------------------------------
        // Bounding box
        // ---------------------------------------------------------
        public BoundingBox GetBoundingBox()
            => new BoundingBox(X, Y, Width, Height, NazwaObj);

        // ---------------------------------------------------------
        // Wierzchołki & krawędzie
        // ---------------------------------------------------------
        public List<XPoint> GetCorners() => GeneratePoints();

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GeneratePoints();
            return new()
            {
                (v[0], v[1]),
                (v[1], v[2]),
                (v[2], v[3]),
                (v[3], v[0])
            };
        }
    }
}

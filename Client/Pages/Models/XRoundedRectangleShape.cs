using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedRectangleShape : IShapeDC
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public string NazwaObj { get; set; } = "Prostokąt z zaokrąglonymi górnymi naroż.";

        private double _scaleFactor = 1.0;

        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        // Punkty robocze
        public List<XPoint> Points { get; set; } = new();

        // Punkty nominalne (geometria pierwotna)
        public List<XPoint> NominalPoints { get; set; } = new();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() =>
            NominalPoints.Select(p => new XPoint(p.X, p.Y)).ToList();

        public XRoundedRectangleShape(
            double x, double y, double width, double height, double radius, double scaleFactor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Radius = radius;
            _scaleFactor = scaleFactor;

            Points = GeneratePoints();
            NominalPoints = Points.Select(p => new XPoint(p.X, p.Y)).ToList();

            Szerokosc = Width;
            Wysokosc = Height;
        }

        // --------------------------------------------------------------------
        // GENEROWANIE PUNKTÓW
        // --------------------------------------------------------------------
        private List<XPoint> GeneratePoints()
        {
            double r = Math.Min(Radius, Math.Min(Width, Height) / 2);

            return new List<XPoint>
            {
                new(X + r, Y),                  // start łuku lewego górnego
                new(X + Width - r, Y),          // przed prawym łukiem
                new(X + Width, Y),              // węzeł łuku prawego
                new(X + Width, Y + Height),
                new(X, Y + Height),
                new(X, Y),                      // węzeł łuku lewego
                new(X + r, Y)                   // domknięcie
            };
        }

        // --------------------------------------------------------------------
        // UPDATE POINTS (np. po edycji użytkownika)
        // --------------------------------------------------------------------
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count == 0)
                return;

            Points = newPoints;

            double minX = newPoints.Min(p => p.X);
            double maxX = newPoints.Max(p => p.X);
            double minY = newPoints.Min(p => p.Y);
            double maxY = newPoints.Max(p => p.Y);

            X = minX;
            Y = minY;
            Width = maxX - minX;
            Height = maxY - minY;

            Szerokosc = Width;
            Wysokosc = Height;

            double maxR = Math.Min(Width, Height) / 2;
            Radius = Math.Min(Radius, maxR);

            Points = GeneratePoints();
        }

        // --------------------------------------------------------------------
        // KLONOWANIE
        // --------------------------------------------------------------------
        public IShapeDC Clone()
        {
            var c = new XRoundedRectangleShape(X, Y, Width, Height, Radius, _scaleFactor);
            c.Points = Points.Select(p => new XPoint(p.X, p.Y)).ToList();
            c.NominalPoints = NominalPoints.Select(p => new XPoint(p.X, p.Y)).ToList();
            return c;
        }

        public List<XPoint> GetVertices()
        {
            double arcCenterY = Y + Radius;

            return new List<XPoint>
            {
                new XPoint(X, Y + Height),
                new XPoint(X + Width, Y + Height),
                new XPoint(X + Width, arcCenterY),
                new XPoint(X, arcCenterY)
            };
        }

        // --------------------------------------------------------------------
        // RYSOWANIE
        // --------------------------------------------------------------------
        public async Task Draw(Canvas2DContext ctx)
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            double r = Math.Min(Radius, Math.Min(Width, Height) / 2);

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(X + r, Y);

            await ctx.LineToAsync(X + Width - r, Y);
            await ctx.ArcToAsync(X + Width, Y, X + Width, Y + r, r);

            await ctx.LineToAsync(X + Width, Y + Height);
            await ctx.LineToAsync(X, Y + Height);

            await ctx.LineToAsync(X, Y + r);
            await ctx.ArcToAsync(X, Y, X + r, Y, r);

            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

        // --------------------------------------------------------------------
        // WŁAŚCIWOŚCI EDYTOWALNE
        // --------------------------------------------------------------------
        public List<EditableProperty> GetEditableProperties() => new()
        {
            new("X", () => X, v => { X = v; Points = GeneratePoints(); }, NazwaObj, true),
            new("Y", () => Y, v => { Y = v; Points = GeneratePoints(); }, NazwaObj, true),
            new("Szerokość", () => Width, v => { Width = v; Points = GeneratePoints(); }, NazwaObj),
            new("Wysokość", () => Height, v => { Height = v; Points = GeneratePoints(); }, NazwaObj),
            new("Promień górnych rogów", () => Radius, v =>
            {
                double maxR = Math.Min(Width, Height) / 2;
                Radius = Math.Min(v, maxR);
                Points = GeneratePoints();
            }, NazwaObj)
        };

        // --------------------------------------------------------------------
        // TRANSFORMACJE
        // --------------------------------------------------------------------
        public void Scale(double factor)
        {
            Width *= factor;
            Height *= factor;
            Radius *= factor;

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
            Radius *= scale;
            Points = GeneratePoints();
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = X * scaleX + offsetX;
            Y = Y * scaleY + offsetY;
            Width *= scaleX;
            Height *= scaleY;
            Radius *= (scaleX + scaleY) / 2.0;
            Points = GeneratePoints();
        }

        // --------------------------------------------------------------------
        // BOUNDING BOX
        // --------------------------------------------------------------------
        public BoundingBox GetBoundingBox()
            => new BoundingBox(X, Y, Width, Height, NazwaObj);
    }
}

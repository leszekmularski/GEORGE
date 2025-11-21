using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedRectangleShape : IShapeDC
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public string NazwaObj { get; set; } = "Prostokąt z zaokrąglonymi górnymi naroż.";

        private double _scaleFactor = 1.0;

        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        public List<XPoint> Points { get; set; }
        public string ID { get; set; } = Guid.NewGuid().ToString();

        public List<XPoint> GetPoints() => Points;

        public XRoundedRectangleShape(double x, double y, double width, double height, double radius, double scaleFactor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Radius = radius;
            _scaleFactor = scaleFactor;

            Points = GeneratePoints();
            Szerokosc = Width;
            Wysokosc = Height;
        }

        // ---------------------------------------------------------
        // 🔥 TYLKO górne naroża są zaokrąglone
        // ---------------------------------------------------------
        private List<XPoint> GeneratePoints()
        {
            return new List<XPoint>
            {
                new(X + Radius, Y),                    // start łuku lewego górnego
                new(X + Width - Radius, Y),            // przed prawym łukiem
                new(X + Width, Y),                     // prawy górny – punkt łuku
                new(X + Width, Y + Height),            // prawy dolny
                new(X, Y + Height),                    // lewy dolny
                new(X, Y),                             // lewy górny (punkt łuku)
                new(X + Radius, Y)                     // domknięcie
            };
        }

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 2)
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

            double maxRadius = Math.Min(Width, Height) / 2;
            Radius = Math.Min(Radius, maxRadius);

            Points = GeneratePoints();
        }

        public IShapeDC Clone()
        {
            return new XRoundedRectangleShape(X, Y, Width, Height, Radius, _scaleFactor);
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();

            // Start od lewego górnego łuku
            await ctx.MoveToAsync(X + Radius, Y);

            // GÓRNA PROSTA
            await ctx.LineToAsync(X + Width - Radius, Y);

            // ⤵️ Prawy górny zaokrąglony
            await ctx.ArcToAsync(X + Width, Y, X + Width, Y + Radius, Radius);

            // PRAWA PROSTA (bez zaokrąglenia na dole)
            await ctx.LineToAsync(X + Width, Y + Height);

            // DOLNA PROSTA
            await ctx.LineToAsync(X, Y + Height);

            // LEWA PROSTA (bez zaokrąglenia na dole)
            await ctx.LineToAsync(X, Y + Radius);

            // ⤴️ Lewy górny zaokrąglony
            await ctx.ArcToAsync(X, Y, X + Radius, Y, Radius);

            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

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

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Width, Height, NazwaObj);
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

        public List<XPoint> GetVertices() => GeneratePoints();

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GeneratePoints();
            return new List<(XPoint, XPoint)>
            {
                (v[0], v[1]),
                (v[1], v[2]),
                (v[2], v[3]),
                (v[3], v[4]),
                (v[4], v[5]),
                (v[5], v[0])
            };
        }
    }
}

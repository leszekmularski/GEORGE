using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedTopRectangleShape : IShapeDC
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double ArcHeight { get; set; }

        public double Szerokosc { get => Width; set => Width = value; }
        public double Wysokosc { get => Height; set => Height = value; }

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() => NominalPoints.Select(p => p.Clone()).ToList();

        private readonly double _scaleFactor;
        public string NazwaObj { get; set; } = "Prostokąt z wypukłym łukiem u góry";

        public XRoundedTopRectangleShape(double x, double y, double width, double height,
                                         double radius = 0, double arcHeight = 0, double scaleFactor = 1.0)
        {
            X = x;
            Y = y;
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            _scaleFactor = scaleFactor;

            if (arcHeight <= 0)
                ArcHeight = Math.Min(Height / 2, Width / 2);
            else
                ArcHeight = Math.Min(arcHeight, Height);

            ArcHeight = Math.Max(0, ArcHeight);

            if (radius <= 0)
                Radius = ArcHeight;
            else
                Radius = Math.Min(radius, Width / 2);

            CalculatePointsFromProperties();
        }

        // ===========================
        // Oblicz punkty zgodne z Canvas
        // ===========================
        private void CalculatePointsFromProperties()
        {
            Points = GenerateCompleteOutline();
            NormalizeToPositiveQuadrant(); 
            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }


        // ===========================
        // Oblicz środek łuku i kąty start/end
        // ===========================
        private (double arcCenterX, double arcCenterY, double startAngle, double endAngle) CalculateArcGeometry()
        {
            double arcStartY = Y + ArcHeight;
            double arcCenterX = X + Width / 2.0;
            double halfWidth = Width / 2;

            double arcCenterY;
            if (Radius >= halfWidth)
                arcCenterY = arcStartY + Math.Sqrt(Radius * Radius - halfWidth * halfWidth);
            else
                arcCenterY = arcStartY + Radius;

            double dxRight = (X + Width) - arcCenterX;
            double dyRight = arcStartY - arcCenterY;
            double startAngle = Math.Atan2(dyRight, dxRight);

            double dxLeft = X - arcCenterX;
            double dyLeft = arcStartY - arcCenterY;
            double endAngle = Math.Atan2(dyLeft, dxLeft);

            if (startAngle > endAngle)
                endAngle += 2 * Math.PI;

            return (arcCenterX, arcCenterY, startAngle, endAngle);
        }

        // ===========================
        // Generowanie punktów wzdłuż łuku i boków
        // ===========================
        private List<XPoint> GenerateCompleteOutline()
        {
            var outline = new List<XPoint>();

            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcStartY = Y + ArcHeight;

            var (arcCenterX, arcCenterY, startAngle, endAngle) = CalculateArcGeometry();

            // Lewy dolny
            outline.Add(new XPoint(leftX, bottomY));
            // Prawy dolny
            outline.Add(new XPoint(rightX, bottomY));
            // Prawy górny (początek łuku)
            outline.Add(new XPoint(rightX, arcStartY));

            // Łuk (8 punktów dla spójności)
            int segments = 8;
            for (int i = 0; i <= segments; i++)
            {
                double t = i / (double)segments;
                double angle = startAngle + t * (endAngle - startAngle);
                double x = arcCenterX + Radius * Math.Cos(angle);
                double y = arcCenterY - Radius * Math.Sin(angle); // odbicie osi Y
                outline.Add(new XPoint(x, y));
            }

            // Lewy górny
            outline.Add(new XPoint(leftX, arcStartY));
            // Zamknięcie
            outline.Add(new XPoint(leftX, bottomY));

            outline.Reverse();

            return outline;
        }

        private void NormalizeToPositiveQuadrant()
        {
            if (Points == null || Points.Count == 0) return;

            double minX = Points.Min(p => p.X);
            double minY = Points.Min(p => p.Y);

            double offsetX = minX < 0 ? -minX : 0;
            double offsetY = minY < 0 ? -minY : 0;

            // Przesuwamy wszystkie punkty
            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();

            // Aktualizujemy też pozycję kształtu
            X += offsetX;
            Y += offsetY;

            // Nominalne też odbijamy, żeby były spójne
            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }


        // ===========================
        // Rysowanie Canvas
        // ===========================
        public async Task Draw(Canvas2DContext ctx)
        {
            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcStartY = Y + ArcHeight;

            var (arcCenterX, arcCenterY, startAngle, endAngle) = CalculateArcGeometry();

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));
            await ctx.BeginPathAsync();

            await ctx.MoveToAsync(leftX, bottomY);
            await ctx.LineToAsync(rightX, bottomY);
            await ctx.LineToAsync(rightX, arcStartY);

            await ctx.ArcAsync(arcCenterX, arcCenterY, Radius, startAngle, endAngle, true);

            await ctx.LineToAsync(leftX, bottomY);
            await ctx.StrokeAsync();
        }

        // ===========================
        // Bounding box i wierzchołki
        // ===========================
        public BoundingBox GetBoundingBox() => new BoundingBox(X, Y, Width, Height, NazwaObj);

        public List<XPoint> GetVertices() => GenerateCompleteOutline();

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GetVertices();
            var edges = new List<(XPoint, XPoint)>();
            for (int i = 0; i < v.Count - 1; i++)
                edges.Add((v[i], v[i + 1]));
            edges.Add((v[^1], v[0]));
            return edges;
        }

        // ===========================
        // Modyfikacja punktów
        // ===========================
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 5) return;
            Points = newPoints.ToList();

            // Przelicz właściwości z punktów
            X = Points.Min(p => p.X);
            Y = Points.Min(p => p.Y);
            Width = Points.Max(p => p.X) - X;
            Height = Points.Max(p => p.Y) - Y;

            ArcHeight = Points[Points.Count / 2].Y - Y;
            Radius = Math.Sqrt(Math.Pow(Points[Points.Count / 2].X - (X + Width / 2), 2) +
                               Math.Pow(Points[Points.Count / 2].Y - (Y + ArcHeight), 2));

            CalculatePointsFromProperties();
        }

        // ===========================
        // Transformacje
        // ===========================
        public void Scale(double factor)
        {
            if (factor == 0) return;
            X *= factor; Y *= factor;
            Width *= factor; Height *= factor;
            Radius *= factor; ArcHeight *= factor;
            CalculatePointsFromProperties();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX; Y += offsetY;
            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();
            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }

        public void Transform(double scale, double offsetX, double offsetY) => Transform(scale, scale, offsetX, offsetY);

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            double scale = (scaleX + scaleY) / 2.0;
            X = X * scale + offsetX;
            Y = Y * scale + offsetY;
            Width *= scale; Height *= scale;
            Radius *= scale; ArcHeight *= scale;

            Width = Math.Max(10, Width);
            Height = Math.Max(10, Height);
            Radius = Math.Max(5, Width / 2);
            ArcHeight = Math.Max(5, Radius);
            ArcHeight = Math.Min(ArcHeight, Height);

            CalculatePointsFromProperties();
        }

        public IShapeDC Clone()
        {
            return new XRoundedTopRectangleShape(X, Y, Width, Height, Radius, ArcHeight, _scaleFactor)
            {
                ID = Guid.NewGuid().ToString(),
                Points = Points.Select(p => p.Clone()).ToList(),
                NominalPoints = NominalPoints.Select(p => p.Clone()).ToList(),
                NazwaObj = NazwaObj
            };
        }

        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("Pozycja X", () => X, v => { X = v; CalculatePointsFromProperties(); }, NazwaObj, true),
            new EditableProperty("Pozycja Y", () => Y, v => { Y = v; CalculatePointsFromProperties(); }, NazwaObj, true),
            new EditableProperty("Szerokość", () => Width, v => { Width = Math.Max(10, v); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Wysokość", () => Height, v => { Height = Math.Max(10, v); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Promień łuku", () => Radius, v => { Radius = Math.Max(5, Math.Min(v, Width / 2)); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Wysokość łuku", () => ArcHeight, v => { ArcHeight = Math.Clamp(v, 5, Height); CalculatePointsFromProperties(); }, NazwaObj),
        };
    }
}

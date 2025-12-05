using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedTopRectangleShape : IShapeDC
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        // geometry core
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }

        public double Szerokosc { get => Width; set => Width = value; }
        public double Wysokosc { get => Height; set => Height = value; }

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() =>
            NominalPoints.Select(p => p.Clone()).ToList();

        private readonly double _scaleFactor;
        public string NazwaObj { get; set; } = "Prostokąt z zaokr. u góry";

        // =====================================================================
        //  Konstruktor
        // =====================================================================
        public XRoundedTopRectangleShape(double x, double y, double width, double height, double radius, double scaleFactor = 1.0)
        {
            X = x;
            Y = y;

            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            Radius = Math.Max(0, radius);

            _scaleFactor = scaleFactor;

            ClampRadius();
            RegeneratePointsFromGeometry();
        }

        // =====================================================================
        //  Integral geometry synchronizer
        // =====================================================================
        private void RegeneratePointsFromGeometry()
        {
            ClampRadius();

            // 0 = lewy dolny
            // 1 = prawy dolny
            // 2 = punkt środkowy łuku (góra)
            Points = new List<XPoint>
            {
                new XPoint(X, Y + Height),
                new XPoint(X + Width, Y + Height),
                new XPoint(X + Width / 2.0, Y + Radius)
            };

            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }

        // =====================================================================
        //  UpdatePoints — przeciąganie przez użytkownika
        // =====================================================================
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 3)
                return;

            Points = newPoints.ToList();

            double minX = Points.Min(p => p.X);
            double maxX = Points.Max(p => p.X);
            double minY = Points.Min(p => p.Y);
            double maxY = Points.Max(p => p.Y);

            X = minX;
            Width = Math.Abs(maxX - minX);

            // Punkt łuku
            var arcPoint = Points[2];

            double centerX = X + Width / 2.0;

            // radius jako odległość pozioma od środka
            double assumedRadius = Math.Abs(arcPoint.X - centerX);

            // dolna krawędź
            double bottomY = maxY;

            // wysokość wyliczona
            double computedHeight = bottomY - (arcPoint.Y - assumedRadius);
            if (computedHeight <= 0)
                computedHeight = Height;

            Height = computedHeight;

            // vertical difference determines top arc
            double verticalDist = bottomY - arcPoint.Y;
            Radius = Math.Min(Math.Min(Width / 2.0, verticalDist), Math.Max(2.0, assumedRadius));

            Y = arcPoint.Y - Radius;

            ClampRadius();
            RegeneratePointsFromGeometry();
        }

        // =====================================================================
        private void ClampRadius()
        {
            if (Width <= 0) Width = 1;
            if (Height <= 0) Height = 1;

            Radius = Math.Max(0, Radius);
            Radius = Math.Min(Radius, Width / 2.0);
            Radius = Math.Min(Radius, Height);
        }

        // =====================================================================
        //  Drawing
        // =====================================================================
        public async Task Draw(Canvas2DContext ctx)
        {
            ClampRadius();
            RegeneratePointsFromGeometry();

            double arcCenterX = X + Width / 2.0;
            double arcCenterY = Y + Radius;

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));
            await ctx.BeginPathAsync();

            // bottom
            await ctx.MoveToAsync(X, Y + Height);
            await ctx.LineToAsync(X + Width, Y + Height);

            // right wall
            await ctx.LineToAsync(X + Width, arcCenterY);

            // top arc
            await ctx.ArcAsync(arcCenterX, arcCenterY, Radius, 0, Math.PI, true);

            // left wall
            await ctx.LineToAsync(X, arcCenterY);
            await ctx.LineToAsync(X, Y + Height);

            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

        // =====================================================================
        //  Geometry helpers
        // =====================================================================
        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Width, Height, NazwaObj);
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

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GetVertices();
            var edges = new List<(XPoint, XPoint)>();
            for (int i = 0; i < v.Count - 1; i++)
                edges.Add((v[i], v[i + 1]));

            edges.Add((v[^1], v[0]));
            return edges;
        }

        // =====================================================================
        //  Transformacje
        // =====================================================================
        public void Scale(double factor)
        {
            if (factor == 0) return;
            X *= factor;
            Y *= factor;
            Width *= factor;
            Height *= factor;
            Radius *= factor;

            RegeneratePointsFromGeometry();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();
            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            Transform(scale, scale, offsetX, offsetY);
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = X * scaleX + offsetX;
            Y = Y * scaleY + offsetY;
            Width *= scaleX;
            Height *= scaleY;
            Radius *= (scaleX + scaleY) / 2.0;

            RegeneratePointsFromGeometry();
        }

        // =====================================================================
        //  Clone
        // =====================================================================
        public IShapeDC Clone()
        {
            var c = new XRoundedTopRectangleShape(X, Y, Width, Height, Radius, _scaleFactor)
            {
                Points = Points.Select(p => p.Clone()).ToList(),
                NominalPoints = NominalPoints.Select(p => p.Clone()).ToList(),
                NazwaObj = NazwaObj
            };

            c.ID = Guid.NewGuid().ToString();
            return c;
        }

        // =====================================================================
        //  Editable properties
        // =====================================================================
        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("X", () => X, v => { X = v; RegeneratePointsFromGeometry(); }, NazwaObj, true),
            new EditableProperty("Y", () => Y, v => { Y = v; RegeneratePointsFromGeometry(); }, NazwaObj, true),
            new EditableProperty("Szerokość", () => Width, v => { Width = v; ClampRadius(); RegeneratePointsFromGeometry(); }, NazwaObj),
            new EditableProperty("Wysokość", () => Height, v => { Height = v; ClampRadius(); RegeneratePointsFromGeometry(); }, NazwaObj),
            new EditableProperty("Promień łuku", () => Radius, v => { Radius = v; ClampRadius(); RegeneratePointsFromGeometry(); }, NazwaObj),
        };
    }
}

using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedRectangleShapeLeft : IShapeDC
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        // Geometria
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }

        public double Szerokosc { get => Width; set => Width = value; }
        public double Wysokosc { get => Height; set => Height = value; }

        public string NazwaObj { get; set; } = "Prostokąt zaokr. lewy górny";

        private readonly double _scaleFactor;

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() =>
            NominalPoints.Select(p => p.Clone()).ToList();

        // =====================================================================
        // Konstruktor
        // =====================================================================
        public XRoundedRectangleShapeLeft(double x, double y, double width, double height, double radius, double scaleFactor = 1.0)
        {
            X = x;
            Y = y;
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            Radius = Math.Max(0, radius);

            _scaleFactor = scaleFactor;

            ClampRadius();
            RegeneratePoints();
        }

        // =====================================================================
        // Generowanie punktów
        // =====================================================================
        private void RegeneratePoints()
        {
            ClampRadius();

            Points = new List<XPoint>
            {
                new XPoint(X + Radius, Y),        // 0 - górny start po łuku
                new XPoint(X + Width, Y),         // 1 - prawy górny
                new XPoint(X + Width, Y + Height),// 2 - prawy dolny
                new XPoint(X, Y + Height),        // 3 - lewy dolny
                new XPoint(X, Y + Radius),        // 4 - przed łukiem
            };

            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }

        private void ClampRadius()
        {
            Radius = Math.Max(0, Radius);
            double maxR = Math.Min(Width, Height);
            Radius = Math.Min(Radius, maxR);
        }

        // =====================================================================
        // UpdatePoints (przeciąganie myszą)
        // =====================================================================
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 2)
                return;

            Points = newPoints.ToList();

            double minX = Points.Min(p => p.X);
            double maxX = Points.Max(p => p.X);
            double minY = Points.Min(p => p.Y);
            double maxY = Points.Max(p => p.Y);

            X = minX;
            Y = minY;
            Width = maxX - minX;
            Height = maxY - minY;

            ClampRadius();
            RegeneratePoints();
        }

        // =====================================================================
        // Rysowanie
        // =====================================================================
        public async Task Draw(Canvas2DContext ctx)
        {
            ClampRadius();
            RegeneratePoints();

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();

            // góra od łuku do prawej
            await ctx.MoveToAsync(X + Radius, Y);
            await ctx.LineToAsync(X + Width, Y);

            // prawa krawędź
            await ctx.LineToAsync(X + Width, Y + Height);

            // dół
            await ctx.LineToAsync(X, Y + Height);

            // lewa krawędź (od dołu do początku łuku)
            await ctx.LineToAsync(X, Y + Radius);

            // łuk górny lewy
            await ctx.ArcToAsync(X, Y, X + Radius, Y, Radius);

            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

        // =====================================================================
        // Właściwości edytowalne
        // =====================================================================
        public List<EditableProperty> GetEditableProperties() => new()
        {
            new("X", () => X, v => { X = v; RegeneratePoints(); }, NazwaObj, true),
            new("Y", () => Y, v => { Y = v; RegeneratePoints(); }, NazwaObj, true),
            new("Szerokość", () => Width, v => { Width = v; ClampRadius(); RegeneratePoints(); }, NazwaObj),
            new("Wysokość", () => Height, v => { Height = v; ClampRadius(); RegeneratePoints(); }, NazwaObj),
            new("Promień", () => Radius, v => { Radius = v; ClampRadius(); RegeneratePoints(); }, NazwaObj),
        };

        // =====================================================================
        // Transformacje
        // =====================================================================
        public void Scale(double factor)
        {
            Width *= factor;
            Height *= factor;
            Radius *= factor;

            RegeneratePoints();
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

            RegeneratePoints();
        }

        // =====================================================================
        // Bounding / Clone
        // =====================================================================
        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Width, Height, NazwaObj);
        }

        public IShapeDC Clone()
        {
            return new XRoundedRectangleShapeLeft(X, Y, Width, Height, Radius, _scaleFactor)
            {
                Points = Points.Select(p => p.Clone()).ToList(),
                NominalPoints = NominalPoints.Select(p => p.Clone()).ToList(),
                NazwaObj = NazwaObj
            };
        }

        // =====================================================================
        // Wierzchołki i krawędzie
        // =====================================================================
        public List<XPoint> GetVertices()
        {
            return new List<XPoint>(Points.Select(p => p.Clone()));
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
    }
}

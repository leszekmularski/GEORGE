using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class ShapeDC : IShapeDC
    {
        public string ID { get; set; }
        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() => NominalPoints.Select(p => p.Clone()).ToList();

        public Task Draw(Canvas2DContext ctx) => Task.CompletedTask;

        public List<EditableProperty> GetEditableProperties() => new();

        public List<ContourSegment> ContourSegments => GetContourSegments();

        // ---------------------------------------------------------
        // Skalowanie – zawsze oparte na punktach nominalnych!
        // ---------------------------------------------------------
        public void Scale(double factor)
        {
            if (NominalPoints.Count == 0)
                NominalPoints = Points.Select(p => p.Clone()).ToList();

            Points = NominalPoints
                .Select(p => new XPoint(p.X * factor, p.Y * factor))
                .ToList();
        }

        public void Move(double offsetX, double offsetY)
        {
            Points = Points
                .Select(p => new XPoint(p.X + offsetX, p.Y + offsetY))
                .ToList();
        }

        public void Transform(double scale, double offsetX, double offsetY)
            => Transform(scale, scale, offsetX, offsetY);

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            Points = Points
                .Select(p => new XPoint(p.X * scaleX + offsetX, p.Y * scaleY + offsetY))
                .ToList();
        }

        // ---------------------------------------------------------
        // Bounding box
        // ---------------------------------------------------------
        public BoundingBox GetBoundingBox()
        {
            if (Points == null || Points.Count == 0)
                return new BoundingBox(0, 0, 0, 0, ID);

            double minX = Points.Min(p => p.X);
            double maxX = Points.Max(p => p.X);
            double minY = Points.Min(p => p.Y);
            double maxY = Points.Max(p => p.Y);

            return new BoundingBox(minX, minY, maxX - minX, maxY - minY, ID);
        }

        // ---------------------------------------------------------
        // Clone
        // ---------------------------------------------------------
        public IShapeDC Clone()
        {
            return new ShapeDC
            {
                ID = this.ID,
                Szerokosc = this.Szerokosc,
                Wysokosc = this.Wysokosc,
                Points = this.Points.Select(p => p.Clone()).ToList(),
                NominalPoints = this.NominalPoints.Select(p => p.Clone()).ToList()
            };
        }

        // ---------------------------------------------------------
        // UpdatePoints
        // ---------------------------------------------------------
        public void UpdatePoints(List<XPoint> newPoints)
        {
            Points = newPoints.Select(p => p.Clone()).ToList();
            NominalPoints = newPoints.Select(p => p.Clone()).ToList();
        }

        // 🔹 Implementacja metody dla ShapeDC
        public List<ContourSegment> GetContourSegments()
        {
            var segments = new List<ContourSegment>();

            if (NominalPoints == null || NominalPoints.Count < 2)
                return segments;

            for (int i = 0; i < NominalPoints.Count; i++)
            {
                var start = NominalPoints[i].Clone();
                var end = NominalPoints[(i + 1) % NominalPoints.Count].Clone(); // zamyka kontur
                segments.Add(new ContourSegment(start, end));
            }

            return segments;
        }

    }
}

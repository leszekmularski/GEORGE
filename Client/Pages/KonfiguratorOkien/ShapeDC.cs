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

        public List<XPoint> GetPoints() => Points;

        public Task Draw(Canvas2DContext ctx)
        {
            // Minimalne rysowanie (możesz rozwinąć później)
            return Task.CompletedTask;
        }

        public List<EditableProperty> GetEditableProperties() => new();

        public void Scale(double factor)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new XPoint(Points[i].X * factor, Points[i].Y * factor);
            }
        }

        public void Move(double offsetX, double offsetY)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new XPoint(Points[i].X + offsetX, Points[i].Y + offsetY);
            }
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            Transform(scale, scale, offsetX, offsetY);
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new XPoint(
                    Points[i].X * scaleX + offsetX,
                    Points[i].Y * scaleY + offsetY
                );
            }
        }

        public BoundingBox GetBoundingBox()
        {
            if (Points == null || Points.Count == 0)
                return new BoundingBox(0, 0, 0, 0, "");

            double minX = Points.Min(p => p.X);
            double maxX = Points.Max(p => p.X);
            double minY = Points.Min(p => p.Y);
            double maxY = Points.Max(p => p.Y);

            return new BoundingBox(minX, minY, maxX, maxY, ID);
        }


        public IShapeDC Clone()
        {
            return new ShapeDC
            {
                ID = this.ID,
                Szerokosc = this.Szerokosc,
                Wysokosc = this.Wysokosc,
                Points = this.Points.Select(p => new XPoint(p.X, p.Y)).ToList()
            };
        }

        public void UpdatePoints(List<XPoint> newPoints)
        {
            Points = newPoints.ToList();
        }
    }

}

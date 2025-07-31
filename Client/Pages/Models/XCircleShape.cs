using Blazor.Extensions.Canvas.Canvas2D;
using DocumentFormat.OpenXml.Drawing.Charts;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    // 🟢 KLASA KOŁA
    public class XCircleShape : IShapeDC
    {
        public double X { get; set; } // Środek okręgu
        public double Y { get; set; }
        public double Radius { get; set; }
        public string NazwaObj { get; set; } = "Okrąg";

        private double _scaleFactor = 1.0; // Początkowa skala = 1.0 (bez skalowania)
        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        public XCircleShape(double x, double y, double radius, double scaleFactor)
        {
            X = x;
            Y = y;
            Radius = radius;
            _scaleFactor = scaleFactor;
        }

        public IShapeDC Clone()
        {
            return new XCircleShape(X, Y, Radius, _scaleFactor);
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.ArcAsync(X, Y, Radius, 0, 2 * Math.PI);
            await ctx.StrokeAsync();
        }

        public List<EditableProperty> GetEditableProperties() => new()
    {
        new EditableProperty("X", () => X, v => X = v, NazwaObj, true),
        new EditableProperty("Y", () => Y, v => Y = v, NazwaObj, true),
        new EditableProperty("Promień", () => Radius, v => Radius = v, NazwaObj)
    };

        public void Scale(double factor)
        {
            Radius *= factor;
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X - Radius, Y - Radius, Radius * 2, Radius * 2, "Okrąg");
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            X = (X * scale) + offsetX;
            Y = (Y * scale) + offsetY;
            Radius *= scale;
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = (X * scaleX) + offsetX;
            Y = (Y * scaleY) + offsetY;

            // Jeśli skalowanie jest niesymetryczne, wybierz średnią lub mniejszą wartość
            double scaleForRadius = Math.Min(scaleX, scaleY);
            Radius *= scaleForRadius;
        }

        /// <summary>
        /// Przybliża okrąg jako wielokąt o podanej liczbie segmentów.
        /// </summary>
        public List<XPoint> GetVertices(int segments = 32)
        {
            var points = new List<XPoint>();

            for (int i = 0; i < segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                points.Add(new XPoint(
                    X + Radius * Math.Cos(angle),
                    Y + Radius * Math.Sin(angle)
                ));
            }

            return points;
        }
    }

}

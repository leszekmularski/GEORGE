using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedRectangleShapeLeft : IShapeDC
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public string NazwaObj { get; set; } = "Prostoką z 1 zaokr. naroż. lewym";

        private double _scaleFactor = 1.0; // Początkowa skala = 1.0 (bez skalowania)
        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        public XRoundedRectangleShapeLeft(double x, double y, double width, double height, double radius, double scaleFactor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Radius = radius;
            _scaleFactor = scaleFactor;
        }

        public IShapeDC Clone()
        {
            return new XRoundedRectangleShapeLeft(X, Y, Width, Height, Radius, _scaleFactor);
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(X + Radius, Y);
            await ctx.LineToAsync(X + Width, Y);
            await ctx.LineToAsync(X + Width, Y + Height);
            await ctx.LineToAsync(X, Y + Height);
            await ctx.LineToAsync(X, Y + Radius);
            await ctx.ArcToAsync(X, Y, X + Radius, Y, Radius);
            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

        public List<EditableProperty> GetEditableProperties() => new()
        {
            new("X", () => X, v => X = v, NazwaObj, true),
            new("Y", () => Y, v => Y = v, NazwaObj, true),
            new("Szerokość", () => Width, v => Width = v, NazwaObj),
            new("Wysokość", () => Height, v => Height = v, NazwaObj),
            new("Promień", () => Radius, v => Radius = v, NazwaObj)
        };

        public void Scale(double factor)
        {
            Width *= factor;
            Height *= factor;
            Radius *= factor;
        }
        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Width, Height, "Prostokąt");
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            X = (X * scale) + offsetX;
            Y = (Y * scale) + offsetY;
            Width *= scale;
            Height *= scale;
            Radius *= scale;
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = (X * scaleX) + offsetX;
            Y = (Y * scaleY) + offsetY;
            Width *= scaleX;
            Height *= scaleY;
            Radius *= (scaleX + scaleY) / 2.0;  // uśrednione skalowanie promienia
        }


        /// <summary>
        /// Zwraca wierzchołki prostokąta z zaokrąglonym lewym górnym rogiem.
        /// (zawiera kluczowe punkty konturu, bez punktów łuku)
        /// </summary>
        public List<XPoint> GetVertices()
        {
            var vertices = new List<XPoint>();

            // Start po prawej stronie łuku
            vertices.Add(new XPoint(X + Radius, Y));           // punkt po łuku

            vertices.Add(new XPoint(X + Width, Y));            // prawy górny
            vertices.Add(new XPoint(X + Width, Y + Height));   // prawy dolny
            vertices.Add(new XPoint(X, Y + Height));           // lewy dolny
            vertices.Add(new XPoint(X, Y + Radius));           // przed łukiem
            vertices.Add(new XPoint(X, Y));                    // narożnik łuku
            vertices.Add(new XPoint(X + Radius, Y));           // domknięcie do punktu początkowego

            return vertices;
        }


    }

}

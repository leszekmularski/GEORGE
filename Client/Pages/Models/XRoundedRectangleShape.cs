using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedRectangleShape : IShapeDC
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public string NazwaObj { get; set; } = "Prostoką z zaokr. naroż.";

        private double _scaleFactor = 1.0; // Początkowa skala = 1.0 (bez skalowania)

        public XRoundedRectangleShape(double x, double y, double width, double height, double radius, double scaleFactor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Radius = radius;
            _scaleFactor = scaleFactor;
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(X + Radius, Y);
            await ctx.LineToAsync(X + Width - Radius, Y);
            await ctx.ArcToAsync(X + Width, Y, X + Width, Y + Radius, Radius);
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

        /// <summary>
        /// Zwraca wierzchołki prostokąta z zaokrąglonymi rogami po lewej i prawej stronie górnej krawędzi.
        /// (zawiera kluczowe punkty konturu, bez dokładnych punktów łuków)
        /// </summary>
        public List<XPoint> GetVertices()
        {
            var vertices = new List<XPoint>();

            // Punkt po lewej stronie łuku (start)
            vertices.Add(new XPoint(X + Radius, Y));                   // Start po lewej stronie łuku

            vertices.Add(new XPoint(X + Width - Radius, Y));           // Przed prawym łukiem
            vertices.Add(new XPoint(X + Width, Y));                    // Prawy górny narożnik
            vertices.Add(new XPoint(X + Width, Y + Radius));           // Po łuku

            vertices.Add(new XPoint(X + Width, Y + Height));           // Prawy dolny
            vertices.Add(new XPoint(X, Y + Height));                   // Lewy dolny

            vertices.Add(new XPoint(X, Y + Radius));                   // Przed lewym łukiem
            vertices.Add(new XPoint(X, Y));                            // Lewy górny narożnik
            vertices.Add(new XPoint(X + Radius, Y));                   // Po łuku (domknięcie)

            return vertices;
        }


    }

}

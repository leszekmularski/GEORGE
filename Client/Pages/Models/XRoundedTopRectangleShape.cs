using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedTopRectangleShape : IShapeDC
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        private double _scaleFactor;
        public string NazwaObj { get; set; } = "Prostokąt z zaokr. naroż.";

        public XRoundedTopRectangleShape(double x, double y, double width, double height, double radius, double scaleFactor)
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
            double arcCenterX = X + Width / 2;
            double arcCenterY = Y + Radius; // Środek łuku znajduje się na wysokości promienia

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();

            // Dolna część prostokąta
            await ctx.MoveToAsync(X, Y + Height);
            await ctx.LineToAsync(X + Width, Y + Height);
            await ctx.LineToAsync(X + Width, arcCenterY);

            // Górny łuk
            await ctx.ArcAsync(arcCenterX, arcCenterY, Radius, 0, Math.PI, true);

            // Dokończenie prostokąta – BEZ GÓRNEJ LINII!
            await ctx.LineToAsync(X, arcCenterY);
            await ctx.LineToAsync(X, Y + Height);

            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Width, Height, "Prostokąt z zaokr. narożami");
        }

        public List<EditableProperty> GetEditableProperties() => new()
    {
        new EditableProperty("X", () => X, v => X = v, NazwaObj, true),
        new EditableProperty("Y", () => Y, v => Y = v, NazwaObj, true),
        new EditableProperty("Szerokość", () => Width, v => Width = v, NazwaObj),
        new EditableProperty("Wysokość", () => Height, v => Height = v, NazwaObj),
        new EditableProperty("Promień łuku", () => Radius, v => Radius = v, NazwaObj)
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

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            return new List<(XPoint, XPoint)>
            {
                (new XPoint(X, Y + Height), new XPoint(X + Width, Y + Height)), // Dolna krawędź
                (new XPoint(X, Y + Radius), new XPoint(X, Y + Height)), // Lewa ściana
                (new XPoint(X + Width, Y + Radius), new XPoint(X + Width, Y + Height)) // Prawa ściana
            };
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
            Radius *= (scaleX + scaleY) / 2.0;
        }


        /// <summary>
        /// Zwraca wierzchołki prostokąta z zaokrąglonymi rogami (jako lista PointDC).
        /// </summary>
        public List<XPoint> GetVertices()
        {
            var vertices = new List<XPoint>();

            double arcCenterX = X + Width / 2;
            double arcCenterY = Y + Radius;

            // Punkty narożników (dolna część prostokąta)
            vertices.Add(new XPoint(X, Y + Height));           // Lewy dolny
            vertices.Add(new XPoint(X + Width, Y + Height));    // Prawy dolny
            vertices.Add(new XPoint(X + Width, arcCenterY));    // Prawy przy łuku
            vertices.Add(new XPoint(X, arcCenterY));            // Lewy przy łuku

            return vertices;
        }


    }

}

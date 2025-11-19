using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    // 🟥 Klasa prostokąta (Rectangle)
    public class XRectangleShape : IShapeDC
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string NazwaObj { get; set; } = "Prostokąt";

        private double _scaleFactor = 1.0; // Początkowa skala = 1.0 (bez skalowania)
        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }
        public List<XPoint> Points { get; set; }
        public List<XPoint> GetPoints() => Points;
        public void UpdatePoints(List<XPoint> newPoints)
        {
            Points = newPoints;
            // Aktualizuj wymiary na podstawie punktów
            if (Points.Count >= 2)
            {
                Szerokosc = Math.Abs(Points[1].X - Points[0].X);
                Wysokosc = Math.Abs(Points[1].Y - Points[0].Y);
            }
        }
        public XRectangleShape(double x, double y, double width, double height, double scaleFactor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _scaleFactor = scaleFactor;
        }
        public IShapeDC Clone()
        {
            return new XRectangleShape(X, Y, Width, Height, _scaleFactor);
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            // 🔥 Generowanie punktów – to jest KLUCZOWE
            Points = new List<XPoint>
        {
            new XPoint(X, Y),                     // lewy-górny
            new XPoint(X + Width, Y),             // prawy-górny
            new XPoint(X + Width, Y + Height),    // prawy-dolny
            new XPoint(X, Y + Height)             // lewy-dolny
        };

            // 🔥 Opcjonalnie – uaktualnij Szerokosc/Wysokosc
            Szerokosc = Width;
            Wysokosc = Height;

            // 🎨 RYSOWANIE
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.RectAsync(X, Y, Width, Height);
            await ctx.StrokeAsync();
        }

        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("X", () => GetBoundingBox().X, _ => { }, NazwaObj, true),
            new EditableProperty("Y", () => GetBoundingBox().Y, _ => { }, NazwaObj, true),
            new EditableProperty("Szerokość", () => Width, v => Width = v, NazwaObj),
            new EditableProperty("Wysokość", () => Height, v => Height = v, NazwaObj)
        };


        public void Scale(double factor)
        {
            Width *= factor;
            Height *= factor;
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

        public XRectangleShape ToRectangleShape()
        {
            return new XRectangleShape(X, Y, Width, Height, _scaleFactor);
        }

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            List<(XPoint, XPoint)> edges = new()
            {
                (new XPoint(X, Y), new XPoint(X + Width, Y)), // Górna krawędź
                (new XPoint(X + Width, Y), new XPoint(X + Width, Y + Height)), // Prawa krawędź
                (new XPoint(X + Width, Y + Height), new XPoint(X, Y + Height)), // Dolna krawędź
                (new XPoint(X, Y + Height), new XPoint(X, Y)) // Lewa krawędź
            };

            return edges;
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            X = (X * scale) + offsetX;
            Y = (Y * scale) + offsetY;
            Width *= scale;
            Height *= scale;
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = (X * scaleX) + offsetX;
            Y = (Y * scaleY) + offsetY;
            Width *= scaleX;
            Height *= scaleY;
        }

        /// <summary>
        /// Zwraca listę wierzchołków prostokąta w kolejności (zgodnie z ruchem wskazówek zegara).
        /// </summary>
        public List<XPoint> GetCorners()
        {
            return new List<XPoint>
        {
            new(X, Y),
            new(X + Width, Y),
            new(X + Width, Y + Height),
            new(X, Y + Height)
        };
        }

    }

}

using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{

    // ⬛ Klasa kwadratu (Square)
    public class XSquareShape : IShapeDC
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Size { get; set; }
        public string NazwaObj { get; set; } = "Kwadrat";

        private double _scaleFactor = 1.0; // Początkowa skala = 1.0 (bez skalowania)
        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }
        public List<XPoint> Points { get; set; }
        public List<XPoint> GetPoints() => Points;
        public XSquareShape(double x, double y, double size, double scaleFactor)
        {
            X = x;
            Y = y;
            Size = size;
            _scaleFactor = scaleFactor;
        }

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 2)
                return;

            Points = newPoints;

            // Zakładamy, że pierwszy punkt to lewy górny róg, a drugi to prawy dolny
            double minX = Math.Min(Points[0].X, Points[1].X);
            double minY = Math.Min(Points[0].Y, Points[1].Y);
            double maxX = Math.Max(Points[0].X, Points[1].X);
            double maxY = Math.Max(Points[0].Y, Points[1].Y);

            // Obliczamy rozmiar jako średnią z szerokości i wysokości, aby zachować proporcje kwadratu
            double width = maxX - minX;
            double height = maxY - minY;
            double newSize = (width + height) / 2;

            // Aktualizujemy właściwości
            X = minX;
            Y = minY;
            Size = newSize;

            // Jeśli mamy więcej punktów, możemy je wykorzystać do dodatkowych obliczeń
            if (Points.Count > 2)
            {
                // Można dodać logikę dla dodatkowych punktów kontrolnych
                // np. do obracania kwadratu lub zmiany proporcji
            }

            // Aktualizacja wymiarów
            Szerokosc = Size;
            Wysokosc = Size;
        }
        public IShapeDC Clone()
        {
            return new XSquareShape(X, Y, Size, _scaleFactor);
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.RectAsync(X, Y, Size, Size);
            await ctx.StrokeAsync();
        }

        public List<EditableProperty> GetEditableProperties() => new()
    {
        new EditableProperty("X", () => X, v => X = v, NazwaObj, true),
        new EditableProperty("Y", () => Y, v => Y = v, NazwaObj, true),
        new EditableProperty("Rozmiar", () => Size, v => Size = v, NazwaObj)
    };

        public void Scale(double factor)
        {
            Size *= factor;
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Size, Size, "Kształt inny");
        }

        public XRectangleShape ToRectangleShape()
        {
            return new XRectangleShape(X, Y, Size, Size, _scaleFactor);
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            // Poprawiona implementacja
            X = (X * scale) + offsetX;
            Y = (Y * scale) + offsetY;
            Size *= scale;
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = (X * scaleX) + offsetX;
            Y = (Y * scaleY) + offsetY;
            Size *= (scaleX + scaleY) / 2.0; // uśrednione skalowanie Size
        }

        /// <summary>
        /// Zwraca listę wierzchołków prostokąta w kolejności (zgodnie z ruchem wskazówek zegara).
        /// </summary>
        public List<XPoint> GetCorners()
        {
            return new List<XPoint>
        {
            new(X, Y),
            new(X + Size, Y),
            new(X + Size, Y + Size),
            new(X, Y + Size)
        };
        }

    }

}

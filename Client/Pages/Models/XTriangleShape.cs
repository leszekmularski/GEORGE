using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class XTriangleShape : IShapeDC
    {
        // Pozycje bazowe trójkąta
        public double BaseX1 { get; set; }
        public double BaseY { get; set; }
        public double BaseWidth { get; set; }
        public double Height { get; set; }
        public string NazwaObj { get; set; } = "Trójkąt";

        private double _scaleFactor = 1.0;

        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        public List<XPoint> Points { get; set; }
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public List<XPoint> GetPoints() => Points;

        // ---------------------------------------------------------
        // 🔥 Konstruktor z automatycznym generowaniem punktów
        // ---------------------------------------------------------
        public XTriangleShape(double startX, double startY, double endX, double endY, double scaleFactor)
        {
            BaseX1 = Math.Min(startX, endX);
            BaseY = Math.Max(startY, endY);
            BaseWidth = Math.Abs(endX - startX);
            Height = Math.Abs(startY - endY);

            _scaleFactor = scaleFactor;

            Points = GeneratePoints();
        }

        // ---------------------------------------------------------
        // 🔥 Funkcja generująca 3 punkty na podstawie parametrów
        // ---------------------------------------------------------
        private List<XPoint> GeneratePoints()
        {
            var apexX = BaseX1 + BaseWidth / 2;
            var apexY = BaseY - Height;
            var baseX2 = BaseX1 + BaseWidth;

            return new List<XPoint>
            {
                new XPoint(apexX, apexY),      // górny wierzchołek
                new XPoint(baseX2, BaseY),     // prawy dolny
                new XPoint(BaseX1, BaseY)      // lewy dolny
            };
        }

        // ---------------------------------------------------------
        // 🔥 Aktualizacja punktów z przeciągania / edycji
        // ---------------------------------------------------------
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count != 3)
                return;

            Points = newPoints;

            XPoint apex = Points[0];
            XPoint rightBase = Points[1];
            XPoint leftBase = Points[2];

            BaseX1 = leftBase.X;
            BaseY = leftBase.Y;

            BaseWidth = rightBase.X - leftBase.X;
            Height = Math.Abs(apex.Y - BaseY);

            // Spójność podstawy
            if (Math.Abs(rightBase.Y - leftBase.Y) > 0.1)
                BaseY = (rightBase.Y + leftBase.Y) / 2;

            // Aktualizacja wymiarów
            Szerokosc = BaseWidth;
            Wysokosc = Height;

            // ✔️ Rekonstrukcja poprawnych punktów
            Points = GeneratePoints();
        }

        // ---------------------------------------------------------
        public IShapeDC Clone()
        {
            return new XTriangleShape(BaseX1, BaseY - Height, BaseX1 + BaseWidth, BaseY, _scaleFactor);
        }

        // ---------------------------------------------------------
        public async Task Draw(Canvas2DContext ctx)
        {
            var apexX = BaseX1 + BaseWidth / 2;
            var apexY = BaseY - Height;
            var baseX2 = BaseX1 + BaseWidth;

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(apexX, apexY);
            await ctx.LineToAsync(baseX2, BaseY);
            await ctx.LineToAsync(BaseX1, BaseY);
            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

        // ---------------------------------------------------------
        public List<EditableProperty> GetEditableProperties() => new()
        {
            new("Lewa podstawa X", () => BaseX1, v => { BaseX1 = v; Points = GeneratePoints(); }, NazwaObj),
            new("Pozycja Y podstawy", () => BaseY, v => { BaseY = v; Points = GeneratePoints(); }, NazwaObj),
            new("Szerokość podstawy", () => BaseWidth, v => { BaseWidth = v; Points = GeneratePoints(); }, NazwaObj),
            new("Wysokość", () => Height, v => { Height = v; Points = GeneratePoints(); }, NazwaObj)
        };

        // ---------------------------------------------------------
        public void Scale(double factor)
        {
            BaseWidth *= factor;
            Height *= factor;

            BaseX1 -= (BaseWidth * (factor - 1)) / 2;
            BaseY += (Height * (factor - 1));

            Points = GeneratePoints();
        }

        public void Scale(double scaleX, double scaleY)
        {
            BaseWidth *= scaleX;
            Height *= scaleY;

            BaseX1 *= scaleX;
            BaseY *= scaleY;

            Points = GeneratePoints();
        }

        // ---------------------------------------------------------
        public void Move(double offsetX, double offsetY)
        {
            BaseX1 += offsetX;
            BaseY += offsetY;

            Points = GeneratePoints();
        }

        // ---------------------------------------------------------
        public BoundingBox GetBoundingBox()
        {
            double minX = BaseX1;
            double minY = BaseY - Height;
            double maxX = BaseX1 + BaseWidth;
            double maxY = BaseY;

            return new BoundingBox(minX, minY, maxX - minX, maxY - minY, NazwaObj);
        }

        // ---------------------------------------------------------
        public List<XPoint> GetVertices() => GeneratePoints();

        // ---------------------------------------------------------
        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GeneratePoints();
            return new List<(XPoint, XPoint)>
            {
                (v[0], v[1]),
                (v[1], v[2]),
                (v[2], v[0])
            };
        }

        // ---------------------------------------------------------
        public void Transform(double scale, double offsetX, double offsetY)
        {
            Scale(scale);
            Move(offsetX, offsetY);
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            Scale(scaleX, scaleY);
            Move(offsetX, offsetY);
        }
    }
}

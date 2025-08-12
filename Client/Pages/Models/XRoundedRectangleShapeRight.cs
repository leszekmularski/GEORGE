using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedRectangleShapeRight : IShapeDC
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public string NazwaObj { get; set; } = "Prostoką z 1 zaokr. naroż. prawym";

        private double _scaleFactor = 1.0; // Początkowa skala = 1.0 (bez skalowania)
        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }
        public List<XPoint> Points { get; set; }
        public List<XPoint> GetPoints() => Points;
        public XRoundedRectangleShapeRight(double x, double y, double width, double height, double radius, double scaleFactor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Radius = radius;
            _scaleFactor = scaleFactor;
        }

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 2)
                return;

            Points = newPoints;

            // Oblicz bounding box na podstawie wszystkich punktów
            double minX = Points.Min(p => p.X);
            double maxX = Points.Max(p => p.X);
            double minY = Points.Min(p => p.Y);
            double maxY = Points.Max(p => p.Y);

            X = minX;
            Y = minY;
            Width = maxX - minX;
            Height = maxY - minY;
            Szerokosc = Width;
            Wysokosc = Height;

            // Punkt narożnika który będzie zaokrąglony (prawy górny)
            XPoint cornerPoint = new XPoint(X + Width, Y);

            // Domyślny promień (1/4 mniejszego wymiaru)
            double maxPossibleRadius = Math.Min(Width, Height);
            Radius = Math.Min(maxPossibleRadius / 4, 50);

            // Analiza punktów kontrolnych łuku
            if (Points.Count >= 3)
            {
                // Punkt 2 - główny punkt kontrolny łuku
                XPoint controlPoint = Points[2];

                // Oblicz promień na podstawie odległości od narożnika
                double dx = controlPoint.X - cornerPoint.X;
                double dy = controlPoint.Y - cornerPoint.Y;
                Radius = Math.Min(Math.Sqrt(dx * dx + dy * dy), maxPossibleRadius);

                // Jeśli mamy więcej punktów, możemy precyzyjniej określić kształt łuku
                if (Points.Count >= 5)
                {
                    // Punkt 3 - punkt styczności łuku z górną krawędzią
                    // Punkt 4 - punkt styczności łuku z prawą krawędzią

                    // Weryfikacja czy punkty są w odpowiednich pozycjach
                    bool validTangentPoints =
                        Math.Abs(Points[3].Y - Y) < 0.1 &&  // Punkt na górnej krawędzi
                        Math.Abs(Points[4].X - (X + Width)) < 0.1; // Punkt na prawej krawędzi

                    if (validTangentPoints)
                    {
                        // Oblicz promień na podstawie punktów styczności
                        double radiusFromTop = (X + Width) - Points[3].X;
                        double radiusFromRight = Points[4].Y - Y;

                        // Uśredniony promień z obu punktów styczności
                        Radius = Math.Min((radiusFromTop + radiusFromRight) / 2, maxPossibleRadius);
                    }
                }

                // Jeśli mamy pełną definicję łuku (7 punktów - krzywa Béziera)
                if (Points.Count == 7)
                {
                    // Punkty 5 i 6 to punkty kontrolne krzywej Béziera
                    // Możemy dokładniej obliczyć promień na podstawie krzywej
                    XPoint startPoint = Points[3]; // Punkt styczny górny
                    XPoint endPoint = Points[4];   // Punkt styczny prawy
                    XPoint control1 = Points[5];
                    XPoint control2 = Points[6];

                    // Oblicz przybliżony promień jako średnią odległości punktów kontrolnych od narożnika
                    double dist1 = CalculateDistance(control1, cornerPoint);
                    double dist2 = CalculateDistance(control2, cornerPoint);
                    Radius = Math.Min((dist1 + dist2) / 2, maxPossibleRadius);
                }
            }

            // Minimalny promień 2px dla czytelności
            Radius = Math.Max(Radius, 2);
        }

        private double CalculateDistance(XPoint p1, XPoint p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public IShapeDC Clone()
        {
            return new XRoundedRectangleShapeRight(X, Y, Width, Height, Radius, _scaleFactor);
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
            await ctx.LineToAsync(X, Y);
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
        /// Zwraca wierzchołki prostokąta z zaokrąglonymi rogami u góry
        /// (nie odwzorowuje punktów łuku, tylko wierzchołki kluczowe)
        /// </summary>
        public List<XPoint> GetVertices()
        {
            var vertices = new List<XPoint>();

            // Lewy górny punkt startowy (po zaokrągleniu)
            vertices.Add(new XPoint(X + Radius, Y));

            // Punkt przed łukiem
            vertices.Add(new XPoint(X + Width - Radius, Y));

            // Punkt po łuku (koniec zaokrąglenia)
            vertices.Add(new XPoint(X + Width, Y + Radius));

            // Prawy dolny
            vertices.Add(new XPoint(X + Width, Y + Height));

            // Lewy dolny
            vertices.Add(new XPoint(X, Y + Height));

            // Lewy górny (punkt przed łukiem)
            vertices.Add(new XPoint(X, Y));

            return vertices;
        }


    }

}

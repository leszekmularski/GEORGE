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
        public List<XPoint> Points { get; set; }
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public List<XPoint> GetPoints() => Points;
        public XRoundedRectangleShapeLeft(double x, double y, double width, double height, double radius, double scaleFactor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Radius = radius;
            _scaleFactor = scaleFactor;

            Points = GetVertices();
        }

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 2)
                return;

            Points = newPoints;

            // Oblicz bounding box na podstawie punktów
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

            // Punkt narożnika który będzie zaokrąglony (lewy górny)
            XPoint cornerPoint = new XPoint(X, Y);

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

                // Jeśli mamy więcej punktów, precyzyjne określenie kształtu łuku
                if (Points.Count >= 5)
                {
                    // Punkt 3 - punkt styczności łuku z górną krawędzią
                    // Punkt 4 - punkt styczności łuku z lewą krawędzią

                    bool validTangentPoints =
                        Math.Abs(Points[3].Y - Y) < 0.1 &&  // Punkt na górnej krawędzi
                        Math.Abs(Points[4].X - X) < 0.1;    // Punkt na lewej krawędzi

                    if (validTangentPoints)
                    {
                        // Oblicz promień na podstawie punktów styczności
                        double radiusFromTop = Points[3].X - X;
                        double radiusFromLeft = Points[4].Y - Y;

                        // Uśredniony promień z obu punktów styczności
                        Radius = Math.Min((radiusFromTop + radiusFromLeft) / 2, maxPossibleRadius);
                    }
                }

                // Pełna definicja łuku (7 punktów - krzywa Béziera)
                if (Points.Count == 7)
                {
                    // Punkty 5 i 6 to punkty kontrolne krzywej Béziera
                    XPoint startPoint = Points[3]; // Punkt styczny górny
                    XPoint endPoint = Points[4];   // Punkt styczny lewy
                    XPoint control1 = Points[5];
                    XPoint control2 = Points[6];

                    // Oblicz przybliżony promień
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

        // Generuje pełną listę punktów kontrolnych dla lewego górnego zaokrąglenia
        public void GenerateFullControlPoints()
        {
            // Punkt narożnika
            double cornerX = X;
            double cornerY = Y;

            // Punkt styczny z górną krawędzią
            XPoint topTangent = new XPoint(cornerX + Radius, cornerY);

            // Punkt styczny z lewą krawędzią
            XPoint leftTangent = new XPoint(cornerX, cornerY + Radius);

            // Punkty kontrolne Béziera (przybliżone)
            XPoint control1 = new XPoint(cornerX + Radius * 0.15, cornerY + Radius * 0.55);
            XPoint control2 = new XPoint(cornerX + Radius * 0.55, cornerY + Radius * 0.15);

            Points = new List<XPoint>
    {
        new XPoint(X + Width, Y),          // Prawy górny
        new XPoint(X + Width, Y + Height), // Prawy dolny
        new XPoint((topTangent.X + leftTangent.X)/2, (topTangent.Y + leftTangent.Y)/2), // Środek łuku
        topTangent,  // Punkt styczny górny
        leftTangent, // Punkt styczny lewy
        control1,    // Pierwszy punkt kontrolny Béziera
        control2     // Drugi punkt kontrolny Béziera
    };
        }

        // Sprawdza poprawność punktów kontrolnych łuku
        public bool ValidateArcPoints()
        {
            if (Points.Count < 7) return false;

            // Sprawdź czy punkty styczności są w odpowiednich miejscach
            bool topTangentOk = Math.Abs(Points[3].Y - Y) < 0.1;
            bool leftTangentOk = Math.Abs(Points[4].X - X) < 0.1;

            // Sprawdź czy punkty kontrolne są w odpowiednim obszarze
            bool control1Ok = Points[5].X > X && Points[5].Y > Y;
            bool control2Ok = Points[6].X > X && Points[6].Y > Y;

            return topTangentOk && leftTangentOk && control1Ok && control2Ok;
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

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GetVertices();
            var edges = new List<(XPoint, XPoint)>();

            for (int i = 0; i < v.Count - 1; i++)
                edges.Add((v[i], v[i + 1]));

            // domknięcie konturu
            edges.Add((v[^1], v[0]));

            return edges;
        }


    }

}

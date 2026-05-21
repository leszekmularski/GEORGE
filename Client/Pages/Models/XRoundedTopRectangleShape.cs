using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedTopRectangleShape : IShapeDC
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double ArcHeight { get; set; }

        public double Szerokosc { get => Width; set => Width = value; }
        public double Wysokosc { get => Height; set => Height = value; }

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() => NominalPoints.Select(p => p.Clone()).ToList();

        private readonly double _scaleFactor;
        public string NazwaObj { get; set; } = "Prostokąt z wypukłym łukiem u góry";
        public int _iloscElementowLuki = 4; // liczba punktów generowanych na łuku dla spójności
        public int IloscElementowLuki
        {
            get => _iloscElementowLuki;
            set
            {
                _iloscElementowLuki = Math.Max(4, value); // min 2
                GenerateCompleteOutline(_iloscElementowLuki); // 🔥 KLUCZOWE
            }
        }

        public List<ContourSegment> ContourSegments => GetContourSegments();

        public XRoundedTopRectangleShape(double x, double y, double width, double height,
                                  double radius = 0, double arcHeight = 0, double scaleFactor = 1.0)
        {
            X = x;
            Y = y;
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            _scaleFactor = scaleFactor;

            // 🔥 POPRAWIONE: ArcHeight nie może przekraczać Height
            if (arcHeight <= 0)
                ArcHeight = Math.Min(Height / 2, Width / 2);
            else
                ArcHeight = Math.Min(arcHeight, Height);

            ArcHeight = Math.Max(5, ArcHeight);

            // 🔥 POPRAWIONE: Radius musi być >= ArcHeight, ale <= Width/2
            if (radius <= 0)
                Radius = Math.Max(ArcHeight, Width / 2); // Minimum takie, żeby zmieścić szerokość
            else
                Radius = Math.Max(radius, ArcHeight); // Radius nie może być mniejszy niż ArcHeight

            Radius = Math.Min(Radius, Width); // Maksymalne ograniczenie
            Radius = Math.Max(5, Radius);

            CalculatePointsFromProperties();
        }

        // ===========================
        // Oblicz punkty zgodne z Canvas
        // ===========================
        private void CalculatePointsFromProperties()
        {
            Points = GenerateCompleteOutline(IloscElementowLuki);
            NormalizeToPositiveQuadrant();
            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }


        // ===========================
        // Oblicz środek łuku i kąty start/end
        // ===========================

        public (double centerX, double centerY, double startAngle, double endAngle) CalculateArcGeometryD()
        {
            double centerX = X + Width / 2;
            double centerY = Y + ArcHeight; // Środek łuku na wysokości ArcHeight

            // Kąty dla łuku górnego (od prawej do lewej)
            double startAngle = 0; // Prawa strona (0°)
            double endAngle = Math.PI; // Lewa strona (180°)

            return (centerX, centerY, startAngle, endAngle);
        }

        public (double centerX, double centerY, double startAngle, double endAngle) CalculateArcGeometry()
        {
            double leftX = X;
            double rightX = X + Width;
            double topY = Y;                     // najwyższy punkt kształtu
            double arcBaseY = Y + ArcHeight;     // wysokość, na której łuk styka się z bokami

           // Przypadek standardowy: łuk półokrągły gdy ArcHeight >= Height
            if (arcBaseY < Y + Height)
            {
                return CalculateArcGeometryD(); // Prosty przypadek, gdy łuk jest "wypukły" i nie przekracza wysokości
            }

            // Trzy punkty definiujące łuk:
            double p1x = leftX;              // lewy punkt
            double p1y = arcBaseY;
            double p2x = X + Width / 2.0;   // górny wierzchołek
            double p2y = topY;
            double p3x = rightX;             // prawy punkt
            double p3y = arcBaseY;

            // Obliczanie środka i promienia okręgu przechodzącego przez trzy punkty
            var (cx, cy, r) = CircleFromThreePoints(p1x, p1y, p2x, p2y, p3x, p3y);

            // Kąty do punktów w radianach
            double angleToRight = Math.Atan2(p3y - cy, p3x - cx);  // kąt do prawego punktu
            double angleToLeft = Math.Atan2(p1y - cy, p1x - cx);   // kąt do lewego punktu

            // Chcemy rysować łuk od prawej do lewej (zgodnie z ruchem wskazówek zegara w canvas)
            // W canvas kąty rosną zgodnie z ruchem wskazówek zegara (bo Y w dół)
            // Dla ctx.ArcAsync(x, y, r, startAngle, endAngle, true) - true oznacza przeciwnie do zegara
            // Ale w canvas "przeciwnie do zegara" = matematycznie zgodnie z zegarem

            // Dla łuku górnego (wypukłego do góry):
            // angleToRight będzie w okolicy -π/2 (lub 3π/2), angleToLeft w okolicy -π/2 też
            // Chcemy iść od prawej do lewej, czyli od większego kąta do mniejszego

            double startAngle = angleToRight;
            double endAngle = angleToLeft;

            // Upewniamy się, że startAngle > endAngle (idziemy od prawej do lewej)
            if (startAngle < endAngle)
                startAngle += 2 * Math.PI;

            Radius = r;
            return (cx, cy, startAngle, endAngle);
        }

        private (double cx, double cy, double radius) CircleFromThreePoints(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            // Wyznacznik dla równania okręgu
            double d = 2 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
            if (Math.Abs(d) < 1e-9)
                throw new InvalidOperationException("Punkty są współliniowe – nie można utworzyć łuku.");

            double ux = ((x1 * x1 + y1 * y1) * (y2 - y3) + (x2 * x2 + y2 * y2) * (y3 - y1) + (x3 * x3 + y3 * y3) * (y1 - y2)) / d;
            double uy = ((x1 * x1 + y1 * y1) * (x3 - x2) + (x2 * x2 + y2 * y2) * (x1 - x3) + (x3 * x3 + y3 * y3) * (x2 - x1)) / d;

            double radius = Math.Sqrt(Math.Pow(x1 - ux, 2) + Math.Pow(y1 - uy, 2));
            return (ux, uy, radius);
        }
       

        // ===========================
        // Generowanie punktów wzdłuż łuku i boków
        // ===========================
        private List<XPoint> GenerateCompleteOutline(int segments)
        {
            var outline = new List<XPoint>();

            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcStartY = Y + ArcHeight;

            var (arcCenterX, arcCenterY, startAngle, endAngle) = CalculateArcGeometry();

            // Lewy dolny
            outline.Add(new XPoint(leftX, bottomY));
            // Prawy dolny
            outline.Add(new XPoint(rightX, bottomY));
            // Prawy górny (początek łuku)
            outline.Add(new XPoint(rightX, arcStartY));

            //// Łuk (6 punktów dla spójności)
            if (segments <= 0) segments = 2;

            //if (Radius > 1000) segments = 5;

            for (int i = 0; i <= segments; i++)
            {
                double t = i / (double)segments;
                double angle = startAngle + t * (endAngle - startAngle);
                double x = arcCenterX + Radius * Math.Cos(angle);
                double y = arcCenterY - Radius * Math.Sin(angle); // odbicie osi Y
                outline.Add(new XPoint(x, y));
            }

            // Lewy górny
            outline.Add(new XPoint(leftX, arcStartY));
            // Zamknięcie
            outline.Add(new XPoint(leftX, bottomY));

            outline.Reverse();

            for (int i = 0; i < outline.Count; i++)
            {
                var p = outline[i];
                outline[i] = new XPoint(Math.Round(p.X, 4), Math.Round(p.Y, 4));
            }

            return outline;

        }

        private void NormalizeToPositiveQuadrant()
        {
            if (Points == null || Points.Count == 0) return;

            double minX = Points.Min(p => p.X);
            double minY = Points.Min(p => p.Y);

            double offsetX = minX < 0 ? -minX : 0;
            double offsetY = minY < 0 ? -minY : 0;

            // Przesuwamy wszystkie punkty
            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();

            // Aktualizujemy też pozycję kształtu
            X += offsetX;
            Y += offsetY;

            // Nominalne też odbijamy, żeby były spójne
            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }


        // ===========================
        // Rysowanie Canvas
        // ===========================
        public async Task Draw(Canvas2DContext ctx)
        {
            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcStartY = Y + ArcHeight;

            var (arcCenterX, arcCenterY, startAngle, endAngle) = CalculateArcGeometry();

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync(3);
            await ctx.BeginPathAsync();

            await ctx.MoveToAsync(leftX, bottomY);
            await ctx.LineToAsync(rightX, bottomY);
            await ctx.LineToAsync(rightX, arcStartY);

            await ctx.ArcAsync(arcCenterX, arcCenterY, Radius, startAngle, endAngle, true);

            await ctx.LineToAsync(leftX, bottomY);
            await ctx.StrokeAsync();
        }

        // ===========================
        // Bounding box i wierzchołki
        // ===========================
        public BoundingBox GetBoundingBox() => new BoundingBox(X, Y, Width, Height, NazwaObj);

        public List<XPoint> GetVertices() => GenerateCompleteOutline(IloscElementowLuki);

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GetVertices();
            var edges = new List<(XPoint, XPoint)>();
            for (int i = 0; i < v.Count - 1; i++)
                edges.Add((v[i], v[i + 1]));
            edges.Add((v[^1], v[0]));
            return edges;
        }

        // ===========================
        // Modyfikacja punktów
        // ===========================
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 5) return;
            Points = newPoints.ToList();

            // Przelicz właściwości z punktów
            X = Points.Min(p => p.X);
            Y = Points.Min(p => p.Y);
            Width = Points.Max(p => p.X) - X;
            Height = Points.Max(p => p.Y) - Y;

            ArcHeight = Points[Points.Count / 2].Y - Y;
            Radius = Math.Sqrt(Math.Pow(Points[Points.Count / 2].X - (X + Width / 2), 2) +
                               Math.Pow(Points[Points.Count / 2].Y - (Y + ArcHeight), 2));

            CalculatePointsFromProperties();
        }

        // ===========================
        // Transformacje
        // ===========================
        public void Scale(double factor)
        {
            if (factor == 0) return;
            X *= factor; Y *= factor;
            Width *= factor; Height *= factor;
            Radius *= factor; ArcHeight *= factor;
            CalculatePointsFromProperties();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX; Y += offsetY;
            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();
            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }

        public void Transform(double scale, double offsetX, double offsetY) => Transform(scale, scale, offsetX, offsetY);

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            double scale = (scaleX + scaleY) / 2.0;
            X = X * scale + offsetX;
            Y = Y * scale + offsetY;
            Width *= scale; Height *= scale;
            Radius *= scale; ArcHeight *= scale;

            Width = Math.Max(200, Width);
            Height = Math.Max(100, Height);
            Radius = Math.Max(50, Width / 2);
            ArcHeight = Math.Max(50, Radius);
            ArcHeight = Math.Min(ArcHeight, Height);

            CalculatePointsFromProperties();
        }

        public IShapeDC Clone()
        {
            return new XRoundedTopRectangleShape(X, Y, Width, Height, Radius, ArcHeight, _scaleFactor)
            {
                ID = Guid.NewGuid().ToString(),
                Points = Points.Select(p => p.Clone()).ToList(),
                NominalPoints = NominalPoints.Select(p => p.Clone()).ToList(),
                NazwaObj = NazwaObj
            };
        }

        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("Pozycja X: ", () => X, v => { X = v; CalculatePointsFromProperties(); }, NazwaObj, true),
            new EditableProperty("Pozycja Y: ", () => Y, v => { Y = v; CalculatePointsFromProperties(); }, NazwaObj, true),
            new EditableProperty("Szerokość: ", () => Width, v => { Width = Math.Max(200, v); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Wysokość: ", () => Height, v => { Height = Math.Max(100, v); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Promień łuku: ", () => Radius, v => { Radius = Math.Max(50, Math.Min(v, Width / 2)); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Wysokość łuku: ", () => ArcHeight, v => { ArcHeight = Math.Clamp(v, 50, Height); CalculatePointsFromProperties(); }, NazwaObj),
            // 🔥 POPRAWIONE: Tylko wielokrotności liczby 2
            new EditableProperty("Podział na elementy: ", () => IloscElementowLuki, v => {
                int newValue = (int)Math.Round(v / 2.0) * 2; // Zaokrąglij do najbliższej wielokrotności 2
                IloscElementowLuki = Math.Max(2, newValue); // Minimum 2
            }, NazwaObj),
        };

        // 🔹 Generowanie segmentów konturu na podstawie NominalPoints
        public List<ContourSegment> GetContourSegments()
        {
            var segments = new List<ContourSegment>();

            var mode = GetArcMode();

            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcStartY = Y + ArcHeight;

            var arc = CalculateArcGeometry();
            double cx = arc.centerX;
            double cy = arc.centerY;

            var bottomLeft = new XPoint(leftX, bottomY);
            var bottomRight = new XPoint(rightX, bottomY);
            var topRight = new XPoint(rightX, arcStartY);
            var topLeft = new XPoint(leftX, arcStartY);

            // dół
            segments.Add(new ContourSegment(bottomLeft, bottomRight));

            if (mode == ArcMode.Normal)
                // prawa
                segments.Add(new ContourSegment(bottomRight, topRight));

            // łuk
            segments.Add(new ContourSegment(
                topRight,
                topLeft,
                new XPoint(cx, cy),
                Radius,
                true
            ));

            if (mode == ArcMode.Normal)
                // lewa
                segments.Add(new ContourSegment(topLeft, bottomLeft));

            // Console.WriteLine($"[DEBUG] Generated {segments.Count} contour segments for {NazwaObj} (mode: {mode})");

            return segments;
        }

        private XPoint CalculateCentroid(List<XPoint> pts)
        {
            double cx = 0;
            double cy = 0;

            foreach (var p in pts)
            {
                cx += p.X;
                cy += p.Y;
            }

            return new XPoint(cx / pts.Count, cy / pts.Count);
        }

        private ArcMode GetArcMode()
        {
            double ratio = Width / Height;

            // próg możesz później stroić pod swoje dane CAD
            return ratio > 1.8 ? ArcMode.FlattenedTop : ArcMode.Normal;
        }

        private enum ArcMode
        {
            Normal,
            FlattenedTop
        }
    }
}

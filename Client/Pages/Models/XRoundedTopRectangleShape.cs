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

            // Ustaw ArcHeight
            if (arcHeight <= 0)
                ArcHeight = Height / 4;  // Domyślnie 1/4 wysokości
            else
                ArcHeight = Math.Clamp(arcHeight, 5, Height - 10);  // Min 5, max Height-10

            // ZAWSZE obliczaj promień z geometrii (ignoruj parametr radius)
            // R = (w² + 4h²) / (8h)
            Radius = (Width * Width + 4 * ArcHeight * ArcHeight) / (8 * ArcHeight);

            // Upewnij się, że promień jest wystarczający
            Radius = Math.Max(Radius, Width / 2);

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

            //// Przypadek standardowy: łuk półokrągły gdy ArcHeight >= Height
            //if (arcBaseY < Y + Height)
            //{
            //    return CalculateArcGeometryD(); // Prosty przypadek, gdy łuk jest "wypukły" i nie przekracza wysokości
            //}


            return (centerX, centerY, startAngle, endAngle);
        }


        public (double centerX, double centerY, double startAngle, double endAngle) CalculateArcGeometry(double arcHeight = -1)
        {
            double leftX = X;
            double rightX = X + Width;
            double topY = Y;
            ArcHeight = arcHeight > 0 ? arcHeight : ArcHeight;
            double arcBaseY = Y + ArcHeight;
            
            // Przypadek standardowy: łuk półokrągły gdy ArcHeight >= Height
            if (arcBaseY < Y + Height)
            {
                return CalculateArcGeometryD(); // Prosty przypadek, gdy łuk jest "wypukły" i nie przekracza wysokości
            }

            var (cx, cy, r) = CircleFromThreePoints(
                leftX, arcBaseY,
                X + Width / 2.0, topY,
                rightX, arcBaseY
            );

            Radius = r;

            // Kąty do punktów
            double angleRight = Math.Atan2(arcBaseY - cy, rightX - cx);
            double angleLeft = Math.Atan2(arcBaseY - cy, leftX - cx);
            double angleTop = Math.Atan2(topY - cy, (X + Width / 2.0) - cx);

            // Normalizacja
            if (angleRight < 0) angleRight += 2 * Math.PI;
            if (angleLeft < 0) angleLeft += 2 * Math.PI;
            if (angleTop < 0) angleTop += 2 * Math.PI;

            // Sprawdzamy, czy środek jest nad czy pod łukiem
            bool centerBelow = cy > topY;

            double startAngle, endAngle;

            if (centerBelow)
            {
                // Środek poniżej - rysujemy górną część okręgu
                startAngle = angleRight;
                endAngle = angleLeft;

                // Kąt górny powinien być pomiędzy
                if (Math.Abs(startAngle - endAngle) > Math.PI)
                {
                    if (startAngle < endAngle)
                        startAngle += 2 * Math.PI;
                    else
                        endAngle += 2 * Math.PI;
                }
            }
            else
            {
                // Środek powyżej - rysujemy dolną część okręgu
                startAngle = angleLeft;
                endAngle = angleRight;

                if (startAngle > endAngle)
                    endAngle += 2 * Math.PI;
            }

            return (cx, cy, startAngle, endAngle);
        }

        private double CalculateRadiusFromArcGeometry(double width, double arcHeight)
        {
            // Dla symetrycznego łuku gdzie punkty to:
            // (0, h), (w/2, 0), (w, h)
            // gdzie h = arcHeight

            double w = width;
            double h = arcHeight;

            // Promień = (w² + 4h²) / (8h)
            return (w * w + 4 * h * h) / (8 * h);
        }

        private (double cx, double cy, double radius) CircleFromThreePoints(
            double x1, double y1,
            double x2, double y2,
            double x3, double y3)
        {
            // Zakładamy symetryczny układ: x2 = (x1+x3)/2
            double width = x3 - x1;
            double arcHeight = y1 - y2;  // y1 == y3

            double radius = CalculateRadiusFromArcGeometry(width, arcHeight);

            // Środek jest na środku szerokości
            double cx = (x1 + x3) / 2;

            // Środek jest poniżej punktów bazowych o (radius - arcHeight)
            double cy = y1 + (radius - arcHeight);  // Poniżej, bo Y rośnie w dół

            return (cx, cy, radius);
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
                if(Width / 2 <= Height)
                {
                    double y = arcCenterY - Radius * Math.Sin(angle); // odbicie osi Y
                    outline.Add(new XPoint(x, y));
                }
                else
                {
                    double y = arcCenterY + Radius * Math.Sin(angle); // odbicie osi Y
                    outline.Add(new XPoint(x, y));
                }             
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
            new EditableProperty("Promień łuku: ", () => Radius, v => { Radius = Math.Max(50, Math.Min(v, Width / 2)); CalculatePointsFromProperties(); }, NazwaObj, true),
            new EditableProperty("Wysokość łuku: ", () => ArcHeight, v => { ArcHeight = Math.Clamp(v, 0, Height); CalculatePointsFromProperties(); }, NazwaObj),
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

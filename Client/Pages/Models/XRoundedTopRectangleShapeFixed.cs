using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedTopRectangleShapeFixed : IShapeDC
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private double _radius;
        private double _arcHeight;
        private int _iloscElementowLuki = 4;

        public double X
        {
            get => _x;
            set => _x = value;
        }

        public double Y
        {
            get => _y;
            set => _y = value;
        }

        public double Width
        {
            get => _width;
            set => _width = Math.Max(100, value);
        }

        public double Height
        {
            get => _height;
            set => _height = Math.Max(100, value);
        }

        public double Radius
        {
            get => _radius;
            set => _radius = value;
        }

        public double ArcHeight
        {
            get => _arcHeight;
            set
            {
                // ZABEZPIECZENIE: ArcHeight musi być > Width/2
                double minArcHeight = (Width / 2.0) + 1;
                double maxArcHeight = Height - 10;

                double newValue = value;
                if (newValue <= Width / 2.0)
                    newValue = minArcHeight;

                _arcHeight = Math.Clamp(newValue, minArcHeight, maxArcHeight);

                // Jeśli nadal nie spełnia warunku, wymuś prawidłową wartość
                if (_arcHeight <= Width / 2.0)
                    _arcHeight = Width / 2.0 + 1;

                MarkGeometryDirty();
                CalculatePointsFromProperties();
            }
        }

        public double Szerokosc { get => Width; set => Width = value; }
        public double Wysokosc { get => Height; set => Height = value; }

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() => NominalPoints.Select(p => p.Clone()).ToList();

        private readonly double _scaleFactor;
        public string NazwaObj { get; set; } = "Prostokąt z wypukłym łukiem u góry (Fixed)";

        public string? KsztaltModelu { get; set; } = "XRoundedTopRectangleShapeFixed";

        public int IloscElementowLuki
        {
            get => _iloscElementowLuki;
            set
            {
                _iloscElementowLuki = Math.Max(4, value);
                GenerateCompleteOutline(_iloscElementowLuki);
            }
        }

        public List<ContourSegment> ContourSegments => GetContourSegments();

        // Flaga zapobiegająca rekurencji podczas inicjalizacji
        private bool _isInitializing = false;

        // Cache dla CalculateArcGeometry
        private (double centerX, double centerY, double startAngle, double endAngle)? _cachedArcGeometry;
        private double _lastWidth, _lastHeight, _lastArcHeight, _lastX, _lastY;
        private bool _geometryDirty = true;

        public XRoundedTopRectangleShapeFixed(double x, double y, double width, double height,
                          double radius = 0, double arcHeight = 0, double scaleFactor = 1.0)
        {
            _isInitializing = true;

            Console.WriteLine($"[DEBUG] Konstruktor START {NazwaObj}: Width={width}, Height={height}, ArcHeight={arcHeight}");

            _x = x;
            _y = y;
            _width = Math.Max(100, width);
            _height = Math.Max(100, height);
            _scaleFactor = scaleFactor;

            // Ustaw ArcHeight - ZAWSZE większe od Width/2
            double minArcHeight = (Width / 2.0) + 1;
            double maxArcHeight = Height - 10;

            if (arcHeight <= 0)
                _arcHeight = Math.Max(Height / 3, minArcHeight); // Domyślnie 1/3 wysokości, ale nie mniej niż minArcHeight
            else
                _arcHeight = Math.Clamp(arcHeight, minArcHeight, maxArcHeight);

            // Ostateczne zabezpieczenie
            if (_arcHeight <= Width / 2.0)
                _arcHeight = Width / 2.0 + 1;

            // Oblicz promień z geometrii
            _radius = (Width * Width + 4 * ArcHeight * ArcHeight) / (8 * ArcHeight);
            _radius = Math.Max(_radius, Width / 2);

            CalculatePointsFromProperties();

            _isInitializing = false;

            Console.WriteLine($"[DEBUG] Konstruktor END {NazwaObj}: Width={Width}, Height={Height}, ArcHeight={ArcHeight}, MinRequired={minArcHeight}");
        }

        // ===========================
        // Oblicz punkty zgodne z Canvas
        // ===========================
        private void CalculatePointsFromProperties()
        {
            if (_isInitializing) return;

            Console.WriteLine($"[DEBUG] CalculatePointsFromProperties START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");
            Points = GenerateCompleteOutline(IloscElementowLuki);
            NormalizeToPositiveQuadrant();
            NominalPoints = Points.Select(p => p.Clone()).ToList();

            MarkGeometryDirty();

            Console.WriteLine($"[DEBUG] CalculatePointsFromProperties END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");
        }

        // ===========================
        // Oblicz środek łuku i kąty start/end z cache'owaniem
        // ===========================
        public (double centerX, double centerY, double startAngle, double endAngle) CalculateArcGeometry()
        {
            if (_isInitializing)
            {
                return (X + Width / 2, Y + ArcHeight, 0, Math.PI);
            }

            if (!_geometryDirty &&
                _lastWidth == Width &&
                _lastHeight == Height &&
                _lastArcHeight == ArcHeight &&
                _lastX == X &&
                _lastY == Y &&
                _cachedArcGeometry.HasValue)
            {
                return _cachedArcGeometry.Value;
            }

            Console.WriteLine($"[DEBUG] CalculateArcGeometry START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            double leftX = X;
            double rightX = X + Width;
            double topY = Y;
            double arcBaseY = Y + ArcHeight;

            // Zawsze używaj metody z trzema punktami dla łuku wypukłego
            var (cx, cy, r) = CircleFromThreePoints(
                leftX, arcBaseY,
                X + Width / 2.0, topY,
                rightX, arcBaseY
            );

            _radius = r;

            // Kąty do punktów
            double angleRight = Math.Atan2(arcBaseY - cy, rightX - cx);
            double angleLeft = Math.Atan2(arcBaseY - cy, leftX - cx);

            // Normalizacja
            if (angleRight < 0) angleRight += 2 * Math.PI;
            if (angleLeft < 0) angleLeft += 2 * Math.PI;

            double startAngle, endAngle;

            // Dla łuku wypukłego (idziemy od prawej do lewej)
            startAngle = angleRight;
            endAngle = angleLeft;

            if (startAngle > endAngle)
                endAngle += 2 * Math.PI;

            var result = (cx, cy, startAngle, endAngle);

            Console.WriteLine($"[DEBUG] CalculateArcGeometry {NazwaObj}: Height={Height}, ArcHeight={ArcHeight} Radius={Radius}");
            Console.WriteLine($"[DEBUG] CalculateArcGeometry END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            _cachedArcGeometry = result;
            _lastWidth = Width;
            _lastHeight = Height;
            _lastArcHeight = ArcHeight;
            _lastX = X;
            _lastY = Y;
            _geometryDirty = false;

            return result;
        }

        private void MarkGeometryDirty()
        {
            _geometryDirty = true;
        }

        private (double cx, double cy, double radius) CircleFromThreePoints(
            double x1, double y1,
            double x2, double y2,
            double x3, double y3)
        {
            Console.WriteLine($"[DEBUG] CircleFromThreePoints START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            double width = x3 - x1;
            double arcHeight = y1 - y2;
            double radius = CalculateRadiusFromArcGeometry(width, arcHeight);
            double cx = (x1 + x3) / 2;
            double cy = y1 + (radius - arcHeight);

            Console.WriteLine($"[DEBUG] CircleFromThreePoints END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");
            return (cx, cy, radius);
        }

        private double CalculateRadiusFromArcGeometry(double width, double arcHeight)
        {
            double w = width;
            double h = arcHeight;
            return (w * w + 4 * h * h) / (8 * h);
        }

        // ===========================
        // Generowanie punktów wzdłuż łuku i boków
        // ===========================
        private List<XPoint> GenerateCompleteOutline(int segments)
        {
            Console.WriteLine($"[DEBUG] GenerateCompleteOutline START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            var outline = new List<XPoint>();

            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcStartY = Y + ArcHeight;

            var (arcCenterX, arcCenterY, startAngle, endAngle) = CalculateArcGeometry();

            outline.Add(new XPoint(leftX, bottomY));
            outline.Add(new XPoint(rightX, bottomY));
            outline.Add(new XPoint(rightX, arcStartY));

            if (segments <= 0) segments = 2;

            // Generuj łuk od prawej do lewej
            for (int i = 0; i <= segments; i++)
            {
                double t = i / (double)segments;
                double angle = startAngle + t * (endAngle - startAngle);
                double x = arcCenterX + Radius * Math.Cos(angle);
                double y = arcCenterY - Radius * Math.Sin(angle); // Zawsze odejmujemy dla łuku wypukłego
                outline.Add(new XPoint(x, y));
            }

            outline.Add(new XPoint(leftX, arcStartY));
            outline.Add(new XPoint(leftX, bottomY));
            outline.Reverse();

            for (int i = 0; i < outline.Count; i++)
            {
                var p = outline[i];
                outline[i] = new XPoint(Math.Round(p.X, 4), Math.Round(p.Y, 4));
            }

            Console.WriteLine($"[DEBUG] GenerateCompleteOutline END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            return outline;
        }

        private void NormalizeToPositiveQuadrant()
        {
            if (Points == null || Points.Count == 0) return;

            double minX = Points.Min(p => p.X);
            double minY = Points.Min(p => p.Y);

            double offsetX = minX < 0 ? -minX : 0;
            double offsetY = minY < 0 ? -minY : 0;

            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();

            X += offsetX;
            Y += offsetY;

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
            Console.WriteLine($"[DEBUG] UpdatePoints START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            Points = newPoints.ToList();

            X = Points.Min(p => p.X);
            Y = Points.Min(p => p.Y);
            Width = Points.Max(p => p.X) - X;
            Height = Points.Max(p => p.Y) - Y;

            double newArcHeight = Points[Points.Count / 2].Y - Y;

            // Zabezpieczenie: ArcHeight musi być > Width/2
            double minArcHeight = (Width / 2.0) + 1;
            if (newArcHeight <= Width / 2.0)
                newArcHeight = minArcHeight;

            ArcHeight = Math.Clamp(newArcHeight, minArcHeight, Height - 10);

            Radius = Math.Sqrt(Math.Pow(Points[Points.Count / 2].X - (X + Width / 2), 2) +
                               Math.Pow(Points[Points.Count / 2].Y - (Y + ArcHeight), 2));

            MarkGeometryDirty();
            CalculatePointsFromProperties();

            Console.WriteLine($"[DEBUG] UpdatePoints END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");
        }

        // ===========================
        // Transformacje
        // ===========================
        public void Scale(double factor)
        {
            if (factor == 0) return;

            double newWidth = Width * factor;
            double newHeight = Height * factor;
            double newArcHeight = ArcHeight * factor;

            // Zabezpieczenie po skalowaniu
            double minArcHeight = (newWidth / 2.0) + 1;
            if (newArcHeight <= newWidth / 2.0)
                newArcHeight = minArcHeight;

            X *= factor;
            Y *= factor;
            Width = newWidth;
            Height = newHeight;
            ArcHeight = newArcHeight;
            Radius = (Width * Width + 4 * ArcHeight * ArcHeight) / (8 * ArcHeight);

            MarkGeometryDirty();
            CalculatePointsFromProperties();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();
            NominalPoints = Points.Select(p => p.Clone()).ToList();
            MarkGeometryDirty();
        }

        public void Transform(double scale, double offsetX, double offsetY) => Transform(scale, scale, offsetX, offsetY);

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            Console.WriteLine($"[DEBUG] Transform START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            double scale = (scaleX + scaleY) / 2.0;

            double newWidth = Width * scale;
            double newHeight = Height * scale;
            double newArcHeight = ArcHeight * scale;

            // ZABEZPIECZENIE: ArcHeight musi być > Width/2
            double minArcHeight = (newWidth / 2.0) + 1;
            if (newArcHeight <= newWidth / 2.0)
                newArcHeight = minArcHeight;

            // Ogranicz do wysokości
            if (newArcHeight > newHeight - 10)
                newArcHeight = newHeight - 10;

            X = X * scale + offsetX;
            Y = Y * scale + offsetY;
            Width = Math.Max(100, newWidth);
            Height = Math.Max(100, newHeight);
            ArcHeight = newArcHeight;

            // Przelicz promień
            Radius = (Width * Width + 4 * ArcHeight * ArcHeight) / (8 * ArcHeight);
            Radius = Math.Max(Radius, Width / 2);

            MarkGeometryDirty();
            CalculatePointsFromProperties();

            Console.WriteLine($"[DEBUG] Transform END {NazwaObj}: Width={Width}, Height={Height}, ArcHeight={ArcHeight}");
        }

        public IShapeDC Clone()
        {
            return new XRoundedTopRectangleShapeFixed(X, Y, Width, Height, Radius, ArcHeight, _scaleFactor)
            {
                ID = Guid.NewGuid().ToString(),
                Points = Points.Select(p => p.Clone()).ToList(),
                NominalPoints = NominalPoints.Select(p => p.Clone()).ToList(),
                NazwaObj = NazwaObj,
                Radius = Radius,
                ArcHeight = ArcHeight,
                KsztaltModelu = KsztaltModelu,
                Szerokosc = Width,
                Wysokosc = Height,
            };
        }

        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("Pozycja X: ", () => X, v => { X = v; MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj, true, false, false, true),
            new EditableProperty("Pozycja Y: ", () => Y, v => { Y = v; MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj, true, false, false, true),
            new EditableProperty("Szerokość: ", () => Width, v => { Width = Math.Max(100, v); MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Wysokość: ", () => Height, v => { Height = Math.Max(100, v); MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Promień łuku: ", () => Radius, v => { Radius = v; MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj, true),
            new EditableProperty("Wysokość łuku: ", () => ArcHeight, v => { ArcHeight = v; MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Podział na elementy: ", () => IloscElementowLuki, v => {
                int newValue = (int)Math.Round(v / 2.0) * 2;
                IloscElementowLuki = Math.Max(2, newValue);
            }, NazwaObj),
        };

        // ===========================
        // Generowanie segmentów konturu
        // ===========================
        public List<ContourSegment> GetContourSegments()
        {
            if (_isInitializing) return new List<ContourSegment>();

            Console.WriteLine($"[DEBUG] GetContourSegments START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            var segments = new List<ContourSegment>();

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

            segments.Add(new ContourSegment(bottomLeft, bottomRight));
            segments.Add(new ContourSegment(bottomRight, topRight));
            segments.Add(new ContourSegment(topRight, topLeft, new XPoint(cx, cy), Radius, true));
            segments.Add(new ContourSegment(topLeft, bottomLeft));

            Console.WriteLine($"[DEBUG] GetContourSegments END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            return segments;
        }
    }
}
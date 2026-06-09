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
            set
            {
                if (Math.Abs(_width - value) > 0.001)
                {
                    _width = Math.Max(100, value);
                    // Aktualizuj arcHeight jeśli przekracza nowe ograniczenia
                    _arcHeight = Math.Min(_arcHeight, _height / 2);
                    _radius = CalculateRadiusFromArcGeometry(_width, _arcHeight);
                    MarkGeometryDirty();
                    if (!_isUpdating)
                    {
                        _isUpdating = true;
                        CalculatePointsFromProperties();
                        _isUpdating = false;
                    }
                }
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                if (Math.Abs(_height - value) > 0.001)
                {
                    _height = Math.Max(100, value);
                    _arcHeight = Math.Min(_arcHeight, _height / 2);
                    // Radius nie zależy od Height
                    MarkGeometryDirty();
                    if (!_isUpdating)
                    {
                        _isUpdating = true;
                        CalculatePointsFromProperties();
                        _isUpdating = false;
                    }
                }
            }
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
                double newValue = Math.Clamp(value, 5, _height / 2);
                if (Math.Abs(_arcHeight - newValue) > 0.001)
                {
                    _arcHeight = newValue;
                    _radius = CalculateRadiusFromArcGeometry(_width, _arcHeight); // Użyj Width, nie _width
                    MarkGeometryDirty();
                    if (!_isUpdating)
                    {
                        _isUpdating = true;
                        CalculatePointsFromProperties();
                        _isUpdating = false;
                    }
                }
            }
        }

        public double Szerokosc { get => Width; set => Width = value; }
        public double Wysokosc { get => Height; set => Height = value; }

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() => NominalPoints.Select(p => p.Clone()).ToList();

        private readonly double _scaleFactor;
        public string NazwaObj { get; set; } = "Prostokąt z niepłnym łukiem u góry";
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

        private bool _isInitializing = false;
        private bool _isUpdating = false;

        // Cache dla CalculateArcGeometry
        private (double centerX, double centerY, double startAngle, double endAngle)? _cachedArcGeometry;
        private double _lastWidth, _lastHeight, _lastArcHeight, _lastX, _lastY;
        private bool _geometryDirty = true;

        public XRoundedTopRectangleShapeFixed(double x, double y, double width, double height,
                          double radius = 0, double arcHeight = 0, double scaleFactor = 1.0)
        {
            //_isInitializing = true;

            _x = x;
            _y = y;
            _width = Math.Max(100, width);
            _height = Math.Max(100, height);
            _scaleFactor = scaleFactor;

            //// Ustaw ArcHeight - ogranicz do połowy Height
            //if (arcHeight <= 0)
            //    //_arcHeight = Math.Min(_height / 3, _width / 2);
            //    _arcHeight = Math.Min(_height * 0.33, _width / 2);
            //else
            //_arcHeight = Math.Clamp(arcHeight, 5, _height / 2);
            _arcHeight = Math.Min(_height * 0.33, _width / 2);

            // Oblicz promień z geometrii łuku: R = (w² + 4h²) / (8h)
            _radius = CalculateRadiusFromArcGeometry(_width, _arcHeight);
            //_radius = Math.Max(_radius, _width / 2);

            // _isInitializing = false;
            CalculatePointsFromProperties();
        }

        // Poprawione CalculatePointsFromProperties
        private void CalculatePointsFromProperties(double radius = -1, bool blokujArcHeight = false)
        {
            if (_isInitializing) return;

            // Użyj podanego promienia lub obliczonego
            double useRadius = radius > 0 ? radius : _radius;

            Points = GenerateCompleteOutline(IloscElementowLuki, useRadius);

            NormalizeToPositiveQuadrant();
            NominalPoints = Points.Select(p => p.Clone()).ToList();
            MarkGeometryDirty();
        }

        /// <summary>
        /// Oblicza promień łuku kołowego: R = (w² + 4h²) / (8h)
        /// </summary>
        public static double CalculateRadiusFromArcGeometry(double chordWidth, double arcHeight)
        {
            if (arcHeight <= 0) return chordWidth / 2;
            return (chordWidth * chordWidth + 4 * arcHeight * arcHeight) / (8 * arcHeight);
        }

        /// <summary>
        /// Oblicza geometrię łuku - teraz zawsze niepełny łuk na podstawie ArcHeight
        /// </summary>
        public (double centerX, double centerY, double startAngle, double endAngle) CalculateArcGeometry()
        {
            if (_isInitializing)
                return (X + Width / 2, Y + ArcHeight, 0, Math.PI);

            // Sprawdź cache
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

            // Zawsze używaj geometrii z ArcHeight (niepełny łuk)
            double chordWidth = Width;
            double sagitta = ArcHeight;
            double arcBaseY = Y + ArcHeight;  // Podstawa łuku (górna krawędź prostokąta)
            double topY = Y;                   // Wierzchołek łuku

            // Oblicz promień
            _radius = CalculateRadiusFromArcGeometry(chordWidth, sagitta);

            // Środek okręgu jest poniżej wierzchołka łuku
            double centerX = X + Width / 2.0;
            double centerY = topY + _radius;

            double leftX = X;
            double rightX = X + Width;

            // Kąty do punktów bocznych (na górnej krawędzi prostokąta)
            double startAngle = Math.Atan2(arcBaseY - centerY, rightX - centerX);
            double endAngle = Math.Atan2(arcBaseY - centerY, leftX - centerX);

            // Normalizacja kątów do [0, 2*PI]
            if (startAngle < 0) startAngle += 2 * Math.PI;
            if (endAngle < 0) endAngle += 2 * Math.PI;

            // Upewnij się, że startAngle < endAngle
            if (startAngle > endAngle) endAngle += 2 * Math.PI;

            var result = (centerX, centerY, startAngle, endAngle);

            // Zapisz w cache
            _cachedArcGeometry = result;
            _lastWidth = Width;
            _lastHeight = Height;
            _lastArcHeight = ArcHeight;
            _lastX = X;
            _lastY = Y;
            _geometryDirty = false;

            return result;
        }

        public static (double centerX, double centerY, double startAngle, double endAngle) CalculateSimple(
        double width, double arcHeight, double x, double y)
        {
            double radius = CalculateRadiusFromArcGeometry(width, arcHeight);
            double centerX = x + width / 2.0;
            double centerY = y + radius;
            double arcBaseY = y + arcHeight;

            double leftX = x;
            double rightX = x + width;

            double startAngle = Math.Atan2(arcBaseY - centerY, rightX - centerX);
            double endAngle = Math.Atan2(arcBaseY - centerY, leftX - centerX);

            if (startAngle < 0) startAngle += 2 * Math.PI;
            if (endAngle < 0) endAngle += 2 * Math.PI;
            if (startAngle > endAngle) endAngle += 2 * Math.PI;

            return (centerX, centerY, startAngle, endAngle);
        }

        private void MarkGeometryDirty()
        {
            _geometryDirty = true;
        }

        /// <summary>
        /// Generuje punkty polilinii - łuk o wysokości ArcHeight
        /// </summary>
        private List<XPoint> GenerateCompleteOutline(int segments, double radiusX = -1)
        {
            var outline = new List<XPoint>();

            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcBaseY = Y + ArcHeight;  // Górna krawędź prostokąta
            double topArcY = Y;               // Wierzchołek łuku

            if (segments <= 0) segments = _iloscElementowLuki;

            // Oblicz geometrię łuku
            double radius = radiusX > 0 ? radiusX : CalculateRadiusFromArcGeometry(_width, _arcHeight);
            //Console.WriteLine($"Calculated radius: {radius} for width: {Width} and arc height: {ArcHeight}");
            double centerX = X + Width / 2.0;
            double centerY = topArcY + radius;

            double angleRight = Math.Atan2(arcBaseY, rightX - centerX);
            double angleLeft = Math.Atan2(arcBaseY, leftX - centerX);

            if (angleRight < 0) angleRight += 2 * Math.PI;
            if (angleLeft < 0) angleLeft += 2 * Math.PI;
            if (angleRight > angleLeft) angleLeft += 2 * Math.PI;

            // Kolejność: lewy dolny -> prawy dolny -> prawy górny -> łuk -> lewy górny -> zamknięcie
            outline.Add(new XPoint(leftX, bottomY));        // 1. Lewy dolny
            outline.Add(new XPoint(rightX, bottomY));       // 2. Prawy dolny
            outline.Add(new XPoint(rightX, arcBaseY));      // 3. Prawy górny (początek łuku)

            // 4. Punkty łuku (od prawego do lewego)
            for (int i = 1; i < segments; i++)
            {
                double t = i / (double)segments;
                double angle = angleRight + t * (angleLeft - angleRight);
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);
                outline.Add(new XPoint(x, y));
            }

            outline.Add(new XPoint(leftX, arcBaseY));       // 5. Lewy górny (koniec łuku)
            outline.Add(new XPoint(leftX, bottomY));        // 6. Zamknięcie

            // Zaokrąglij współrzędne
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

            if (offsetX != 0 || offsetY != 0)
            {
                Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();
                X += offsetX;
                Y += offsetY;
                NominalPoints = Points.Select(p => p.Clone()).ToList();
            }
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcBaseY = Y + ArcHeight;

            var (arcCenterX, arcCenterY, startAngle, endAngle) = CalculateArcGeometry();

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync(3);
            await ctx.BeginPathAsync();

            // Rysuj kontur
            await ctx.MoveToAsync(leftX, bottomY);          // Start: lewy dolny
            await ctx.LineToAsync(rightX, bottomY);         // Do: prawy dolny
            await ctx.LineToAsync(rightX, arcBaseY);        // Do: prawy górny (początek łuku)
            await ctx.ArcAsync(arcCenterX, arcCenterY, Radius, startAngle, endAngle, true);
            await ctx.LineToAsync(leftX, bottomY);          // Zamknij
            await ctx.StrokeAsync();
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Width, Height + ArcHeight, NazwaObj);
        }

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

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 5) return;

            Points = newPoints.ToList();

            double maxY = Points.Max(p => p.Y);
            double minX = Points.Min(p => p.X);
            double maxX = Points.Max(p => p.X);
            double minY = Points.Min(p => p.Y);

            var topSidePoints = Points.Where(p =>
                (Math.Abs(p.X - minX) < 0.01 || Math.Abs(p.X - maxX) < 0.01) &&
                p.Y > minY && p.Y < maxY).ToList();

            double topRectY = topSidePoints.Any() ? topSidePoints.Min(p => p.Y) : (minY + maxY) / 2;

            X = minX;
            Width = maxX - minX;
            Y = minY;
            ArcHeight = topRectY - minY;
            Height = maxY - topRectY;

            _radius = CalculateRadiusFromArcGeometry(Width, ArcHeight);
            MarkGeometryDirty();
            CalculatePointsFromProperties();

        }

        public void Scale(double factor)
        {
            if (factor == 0) return;
            X *= factor; Y *= factor;
            Width *= factor; Height *= factor;
            Radius *= factor; ArcHeight *= factor;
            MarkGeometryDirty();
            CalculatePointsFromProperties();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX; Y += offsetY;
            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();
            NominalPoints = Points.Select(p => p.Clone()).ToList();
            MarkGeometryDirty();
        }

        /// <summary>
        /// Przesuwa kształt tak, aby lewy dolny róg (początek konturu) był w punkcie (0, 0)
        /// i cały kształt znajdował się w dodatniej ćwiartce układu współrzędnych
        /// </summary>
        public void MoveToOrigin()
        {
            if (Points == null || Points.Count == 0) return;

            // Znajdź lewy dolny róg (pierwszy punkt konturu - punkt startowy)
            double startX = Points[0].X;
            double startY = Points[0].Y;

            // Oblicz przesunięcie potrzebne do przeniesienia punktu startowego do (0, 0)
            double offsetX = -startX;
            double offsetY = -startY;

            // Sprawdź, czy po przesunięciu nie będzie ujemnych wartości
            double minXAfterMove = Points.Min(p => p.X) + offsetX;
            double minYAfterMove = Points.Min(p => p.Y) + offsetY;

            // Jeśli po przesunięciu są ujemne wartości, dostosuj przesunięcie
            if (minXAfterMove < 0)
                offsetX += Math.Abs(minXAfterMove);
            if (minYAfterMove < 0)
                offsetY += Math.Abs(minYAfterMove);

            // Zastosuj przesunięcie
            X += offsetX;
            Y += offsetY;

            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();
            NominalPoints = Points.Select(p => p.Clone()).ToList();

            MarkGeometryDirty();
            // CalculatePointsFromProperties();
        }

        public void Transform(double scale, double offsetX, double offsetY) => Transform(scale, scale, offsetX, offsetY);

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            double scale = (scaleX + scaleY) / 2.0;
            X = X * scale + offsetX;
            Y = Y * scale + offsetY;
            Width = Math.Max(100, Width * scale);
            Height = Math.Max(100, Height * scale);
            Radius = CalculateRadiusFromArcGeometry(_width, _arcHeight);
            ArcHeight = _arcHeight;

            MarkGeometryDirty();
            CalculatePointsFromProperties();
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
            };
        }

        // Poprawione GetEditableProperties
        public List<EditableProperty> GetEditableProperties() => new()
    {
        new EditableProperty("Pozycja X: ", () => X, v => {
            X = v;
            MarkGeometryDirty();
            if (!_isUpdating) {
                _isUpdating = true;
                CalculatePointsFromProperties();
                _isUpdating = false;
            }
        }, NazwaObj, true, false, false, false),

        new EditableProperty("Pozycja Y: ", () => Y, v => {
            Y = v;
            MarkGeometryDirty();
            if (!_isUpdating) {
                _isUpdating = true;
                CalculatePointsFromProperties();
                _isUpdating = false;
            }
        }, NazwaObj, true, false, false, false),

        new EditableProperty("Szerokość: ", () => Width, v => {
            Width = Math.Max(100, v); // To już aktualizuje radius i arcHeight w setterze
        }, NazwaObj),

        new EditableProperty("Wysokość: ", () => Height, v => {
            Height = Math.Max(100, v); // To już aktualizuje w setterze
        }, NazwaObj),

        new EditableProperty("Promień łuku: ", () => Radius, v => { 
            // Promień jest obliczany automatycznie, więc tylko do odczytu
        }, NazwaObj, true),

        new EditableProperty("Wysokość łuku: ", () => ArcHeight, v => {
            ArcHeight = Math.Clamp(v, 5, Height); // Setter sam przeliczy radius
        }, NazwaObj),

        new EditableProperty("Podział na elementy: ", () => IloscElementowLuki, v => {
            int newValue = (int)Math.Round(v / 2.0) * 2;
            IloscElementowLuki = Math.Max(4, newValue);
        }, NazwaObj),
    };

        public List<ContourSegment> GetContourSegments()
        {
            if (_isInitializing) return new List<ContourSegment>();

            var segments = new List<ContourSegment>();

            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcBaseY = Y + ArcHeight;

            var (arcCenterX, arcCenterY, startAngle, endAngle) = CalculateArcGeometry();

            var bottomLeft = new XPoint(leftX, bottomY);
            var bottomRight = new XPoint(rightX, bottomY);
            var topRight = new XPoint(rightX, arcBaseY);
            var topLeft = new XPoint(leftX, arcBaseY);

            segments.Add(new ContourSegment(bottomLeft, bottomRight));
            segments.Add(new ContourSegment(bottomRight, topRight));
            segments.Add(new ContourSegment(
                topRight,
                topLeft,
                new XPoint(arcCenterX, arcCenterY),
                Radius,
                true
            ));
            segments.Add(new ContourSegment(topLeft, bottomLeft));

            return segments;
        }
    }
}
using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    /// <summary>
    /// Prostokąt z półkolistym łukiem u góry.
    /// Niezmiennik: Radius = ArcHeight = Width / 2  →  łuk zawsze 0–180°.
    /// Height >= Width / 2  (żeby półkole się mieściło).
    /// Dzięki temu końce łuku pokrywają się z bokami prostokąta,
    /// a styczne na końcach łuku są pionowe → kąty proste.
    /// </summary>
    public class XRoundedTopRectangleShape : IShapeDC
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private int _iloscElementowLuki = 4;
        private readonly double _scaleFactor;

        // Cache geometrii łuku
        private bool _geometryDirty = true;
        private (double centerX, double centerY, double startAngle, double endAngle)? _cachedArcGeometry;
        private double _lastWidth, _lastHeight, _lastX, _lastY;

        // ============================================================
        // Konstruktory
        // ============================================================
        public XRoundedTopRectangleShape()
        {
            _x = 0;
            _y = 0;
            _width = 200;
            _height = 200;
            _scaleFactor = 1.0;

            EnsureSemicircleInvariant();
            CalculatePointsFromProperties();
        }

        public XRoundedTopRectangleShape(double x, double y, double width, double height,
                                         double radius = 0, double arcHeight = 0, double scaleFactor = 1.0)
        {
            _x = x;
            _y = y;
            _width = Math.Max(100, width);
            _height = Math.Max(100, height);
            _scaleFactor = scaleFactor;

            // Parametry radius / arcHeight są ignorowane – wynikają z Width
            EnsureSemicircleInvariant();
            CalculatePointsFromProperties();
        }

        // ============================================================
        // Właściwości geometryczne
        // ============================================================

        public double X
        {
            get => _x;
            set
            {
                if (_x == value) return;
                _x = value;
                MarkGeometryDirty();
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                if (_y == value) return;
                _y = value;
                MarkGeometryDirty();
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                double newWidth = Math.Max(100, value);
                if (_width == newWidth) return;
                _width = newWidth;
                EnsureSemicircleInvariant();   // ← może podnieść Height
                MarkGeometryDirty();
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                double newHeight = Math.Max(100, value);
                if (_height == newHeight) return;
                _height = newHeight;
                EnsureSemicircleInvariant();   // ← podnosi do Width/2 jeśli trzeba
                MarkGeometryDirty();
            }
        }

        /// <summary>Promień łuku – zawsze Width/2. Setter ignorowany (wynika z Width).</summary>
        public double Radius
        {
            get => _width / 2.0;
            set { /* pochodna od Width – ignorowane */ }
        }

        /// <summary>Strzałka łuku – zawsze Width/2. Setter ignorowany (wynika z Width).</summary>
        public double ArcHeight
        {
            get => _width / 2.0;
            set { /* pochodna od Width – ignorowane */ }
        }

        public double Szerokosc { get => Width; set => Width = value; }
        public double Wysokosc { get => Height; set => Height = value; }

        public int IloscElementowLuki
        {
            get => _iloscElementowLuki;
            set
            {
                int newValue = Math.Max(2, value);
                if (_iloscElementowLuki == newValue) return;
                _iloscElementowLuki = newValue;
                CalculatePointsFromProperties();
            }
        }

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();

        public string NazwaObj { get; set; } = "Prostokąt z półkolistym łukiem u góry";
        public string? KsztaltModelu { get; set; } = "XRoundedTopRectangleShape";

        public List<ContourSegment> ContourSegments => GetContourSegments();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() => NominalPoints.Select(p => p.Clone()).ToList();

        // ============================================================
        // Niezmiennik półkola + cache
        // ============================================================

        private void EnsureSemicircleInvariant()
        {
            // Półkole: promień = Width/2 ⇒ Height musi być >= Width/2
            double minHeight = _width / 2.0;
            if (_height < minHeight)
                _height = minHeight;
        }

        private void MarkGeometryDirty() => _geometryDirty = true;

        // ============================================================
        // Geometria łuku – ZAWSZE półkole
        // ============================================================

        /// <summary>
        /// Zawsze zwraca półkole: kąty 0 → π.
        /// Środek łuku: (X + Width/2, Y + Width/2).
        /// </summary>
        public (double centerX, double centerY, double startAngle, double endAngle) CalculateArcGeometry()
        {
            if (!_geometryDirty &&
                _lastWidth == _width &&
                _lastHeight == _height &&
                _lastX == _x &&
                _lastY == _y &&
                _cachedArcGeometry.HasValue)
            {
                return _cachedArcGeometry.Value;
            }

            double centerX = _x + _width / 2.0;
            double centerY = _y + _width / 2.0;
            var result = (centerX, centerY, 0.0, Math.PI);

            _cachedArcGeometry = result;
            _lastWidth = _width;
            _lastHeight = _height;
            _lastX = _x;
            _lastY = _y;
            _geometryDirty = false;

            return result;
        }

        // ============================================================
        // Generowanie punktów
        // ============================================================

        private void CalculatePointsFromProperties()
        {
            Points = GenerateCompleteOutline(_iloscElementowLuki);
            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }

        private List<XPoint> GenerateCompleteOutline(int segments)
        {
            if (segments < 2) segments = 2;

            double leftX = _x;
            double rightX = _x + _width;
            double bottomY = _y + _height;
            double arcBaseY = _y + _width / 2.0;

            var (cx, cy, a0, a1) = CalculateArcGeometry();
            double radius = _width / 2.0;

            var outline = new List<XPoint>
            {
                new XPoint(leftX,  bottomY),    // BL
                new XPoint(rightX, bottomY),    // BR
                new XPoint(rightX, arcBaseY),   // TR – koniec prawego boku (start łuku)
            };

            // Punkty łuku – tylko wewnętrzne (końce TR i TL dodajemy explicite)
            for (int i = 1; i < segments; i++)
            {
                double t = i / (double)segments;
                double angle = a0 + t * (a1 - a0);
                double x = cx + radius * Math.Cos(angle);
                double y = cy - radius * Math.Sin(angle); // minus – łuk „górą"
                outline.Add(new XPoint(x, y));
            }

            outline.Add(new XPoint(leftX, arcBaseY)); // TL – koniec lewego boku (koniec łuku)

            // Zaokrąglenie współrzędnych
            for (int i = 0; i < outline.Count; i++)
            {
                var p = outline[i];
                outline[i] = new XPoint(Math.Round(p.X, 4), Math.Round(p.Y, 4));
            }

            outline.Reverse();
            return outline;
        }

        // ============================================================
        // Rysowanie na canvasie
        // ============================================================

        public async Task Draw(Canvas2DContext ctx)
        {
            double leftX = _x;
            double rightX = _x + _width;
            double bottomY = _y + _height;
            double arcBaseY = _y + _width / 2.0;

            var (cx, cy, _, _) = CalculateArcGeometry();

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync(3);
            await ctx.BeginPathAsync();

            await ctx.MoveToAsync(leftX, bottomY);                     // BL
            await ctx.LineToAsync(rightX, bottomY);                    // BR
            await ctx.LineToAsync(rightX, arcBaseY);                   // TR
            await ctx.ArcAsync(cx, cy, _width / 2.0, 0, Math.PI, true); // łuk: TR → góra → TL
            await ctx.LineToAsync(leftX, bottomY);                     // TL → BL (domknięcie)

            await ctx.StrokeAsync();
        }

        // ============================================================
        // Bounding box, wierzchołki, krawędzie
        // ============================================================

        public BoundingBox GetBoundingBox() => new BoundingBox(_x, _y, _width, _height, NazwaObj);

        public List<XPoint> GetVertices() => GenerateCompleteOutline(_iloscElementowLuki);

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GetVertices();
            var edges = new List<(XPoint, XPoint)>();
            for (int i = 0; i < v.Count - 1; i++)
                edges.Add((v[i], v[i + 1]));
            edges.Add((v[^1], v[0]));
            return edges;
        }

        // ============================================================
        // Modyfikacja punktów
        // ============================================================

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 5) return;

            double newX = newPoints.Min(p => p.X);
            double newY = newPoints.Min(p => p.Y);
            double newWidth = newPoints.Max(p => p.X) - newX;
            double newHeight = newPoints.Max(p => p.Y) - newY;

            _x = newX;
            _y = newY;
            _width = Math.Max(100, newWidth);
            _height = Math.Max(100, newHeight);

            EnsureSemicircleInvariant();
            MarkGeometryDirty();
            CalculatePointsFromProperties();
        }

        // ============================================================
        // Transformacje
        // ============================================================

        public void Scale(double factor)
        {
            if (factor == 0 || double.IsNaN(factor)) return;

            _x *= factor;
            _y *= factor;
            _width = Math.Max(100, _width * factor);
            _height = Math.Max(100, _height * factor);

            EnsureSemicircleInvariant();
            MarkGeometryDirty();
            CalculatePointsFromProperties();
        }

        public void Move(double offsetX, double offsetY)
        {
            _x += offsetX;
            _y += offsetY;

            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();
            NominalPoints = Points.Select(p => p.Clone()).ToList();
            MarkGeometryDirty();
        }

        public void Transform(double scale, double offsetX, double offsetY)
            => Transform(scale, scale, offsetX, offsetY);

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            double scale = (scaleX + scaleY) / 2.0;

            _x = _x * scale + offsetX;
            _y = _y * scale + offsetY;
            _width = Math.Max(200, _width * scale);
            _height = Math.Max(100, _height * scale);

            EnsureSemicircleInvariant();
            MarkGeometryDirty();
            CalculatePointsFromProperties();
        }

        public void TransformObrys(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            Transform(scaleX, scaleY, offsetX, offsetY);
        }

        public IShapeDC Clone()
        {
            return new XRoundedTopRectangleShape(_x, _y, _width, _height, Radius, ArcHeight, _scaleFactor)
            {
                NazwaObj = NazwaObj,
                ID = this.ID,
                KsztaltModelu = this.KsztaltModelu,
                IloscElementowLuki = _iloscElementowLuki,
                Points = this.Points?.Select(p => new XPoint(p.X, p.Y)).ToList() ?? new(),
                NominalPoints = this.NominalPoints?.Select(p => new XPoint(p.X, p.Y)).ToList() ?? new(),
            };
        }

        // ============================================================
        // Edytowalne właściwości
        // ============================================================

        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("Pozycja X: ", () => X, v => { X = v; CalculatePointsFromProperties(); }, NazwaObj, true, false, false, true),
            new EditableProperty("Pozycja Y: ", () => Y, v => { Y = v; CalculatePointsFromProperties(); }, NazwaObj, true, false, false, true),
            new EditableProperty("Szerokość: ", () => Width, v => { Width = Math.Max(200, v); CalculatePointsFromProperties(); }, NazwaObj, false, false, false, false, true),
            new EditableProperty("Wysokość: ", () => Height, v => { Height = Math.Max(100, v); CalculatePointsFromProperties(); }, NazwaObj, false, false, false, false, true),
            new EditableProperty("Podział na elementy: ", () => IloscElementowLuki, v =>
            {
                int newValue = (int)Math.Round(v / 2.0) * 2;
                IloscElementowLuki = Math.Max(2, newValue);
            }, NazwaObj),
            // „Promień łuku" i „Wysokość łuku" usunięte – są pochodne od Width
        };

        // ============================================================
        // Segmenty konturu
        // ============================================================

        public List<ContourSegment> GetContourSegments()
        {
            double leftX = _x;
            double rightX = _x + _width;
            double bottomY = _y + _height;
            double arcBaseY = _y + _width / 2.0;

            var arc = CalculateArcGeometry();

            var bottomLeft = new XPoint(leftX, bottomY);
            var bottomRight = new XPoint(rightX, bottomY);
            var topRight = new XPoint(rightX, arcBaseY);  // (X+W,  Y+W/2)
            var topLeft = new XPoint(leftX, arcBaseY);  // (X,    Y+W/2)

            return new List<ContourSegment>
            {
                new ContourSegment(bottomLeft, bottomRight),
                new ContourSegment(bottomRight, topRight),
                new ContourSegment(
                    topRight,
                    topLeft,
                    new XPoint(arc.centerX, arc.centerY),
                    _width / 2.0,
                    false),                                  // łuk górą, prawo → lewo
                new ContourSegment(topLeft, bottomLeft),
            };
        }
    }
}
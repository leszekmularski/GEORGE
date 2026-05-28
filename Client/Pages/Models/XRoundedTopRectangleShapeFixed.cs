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
                _width = Math.Max(50, value);
                ValidateArcHeight();
                MarkGeometryDirty();
                CalculatePointsFromProperties();
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                _height = Math.Max(50, value);
                ValidateArcHeight();
                MarkGeometryDirty();
                CalculatePointsFromProperties();
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
                _arcHeight = Math.Clamp(value, 5, Height / 2);
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
                CalculatePointsFromProperties();
            }
        }

        public List<ContourSegment> ContourSegments => GetContourSegments();

        private bool _isInitializing = false;
        private bool _geometryDirty = true;

        public XRoundedTopRectangleShapeFixed(double x, double y, double width, double height,
                          double radius = 0, double arcHeight = 0, double scaleFactor = 1.0)
        {
            _isInitializing = true;

            _x = x;
            _y = y;
            _width = Math.Max(50, width);
            _height = Math.Max(50, height);
            _scaleFactor = scaleFactor;

            // Wysokość łuku - domyślnie 1/3 wysokości
            if (arcHeight <= 0)
                _arcHeight = Math.Min(_height / 3, _width / 2);
            else
                _arcHeight = Math.Clamp(arcHeight, 5, _height / 2);

            // Promień - dla łuku kwadratowego (quadratic curve) nie jest potrzebny
            // Używamy go tylko do celów zgodności
            _radius = _width / 2;

            CalculatePointsFromProperties();
            _isInitializing = false;
        }

        private void ValidateArcHeight()
        {
            double maxArcHeight = Math.Min(Height / 2, Width / 1.5);
            if (_arcHeight > maxArcHeight)
                _arcHeight = maxArcHeight;
            if (_arcHeight < 5)
                _arcHeight = 5;
        }

        private void CalculatePointsFromProperties()
        {
            if (_isInitializing) return;

            var newPoints = GenerateCompleteOutline(IloscElementowLuki);
            if (newPoints != null && newPoints.Count > 0)
            {
                Points = newPoints;
                NominalPoints = Points.Select(p => p.Clone()).ToList();
            }
            else
            {
                Points = CreateFallbackRectangle();
                NominalPoints = Points.Select(p => p.Clone()).ToList();
            }
        }

        private List<XPoint> CreateFallbackRectangle()
        {
            return new List<XPoint>
            {
                new XPoint(X, Y + Height),
                new XPoint(X + Width, Y + Height),
                new XPoint(X + Width, Y),
                new XPoint(X, Y)
            };
        }

        private List<XPoint> GenerateCompleteOutline(int segments)
        {
            var outline = new List<XPoint>();

            try
            {
                double leftX = X;
                double rightX = X + Width;
                double bottomY = Y + Height;
                double topY = Y;

                // Punkt kontrolny dla łuku kwadratowego (jak w SVG)
                double controlX = leftX + Width / 2;
                double controlY = topY - ArcHeight; // Łuk wystaje do góry

                // Rysuj w kolejności: lewy dolny -> prawy dolny -> prawy górny -> łuk -> lewy górny
                outline.Add(new XPoint(leftX, bottomY));           // Lewy dolny
                outline.Add(new XPoint(rightX, bottomY));          // Prawy dolny
                outline.Add(new XPoint(rightX, topY));             // Prawy górny

                // Generuj łuk kwadratowy (od prawego górnego do lewego górnego)
                if (segments <= 0) segments = 20;

                for (int i = 0; i <= segments; i++)
                {
                    double t = i / (double)segments;
                    // Równanie parametryczne dla Quadratic Bezier
                    // B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
                    double x = Math.Pow(1 - t, 2) * rightX + 2 * (1 - t) * t * controlX + Math.Pow(t, 2) * leftX;
                    double y = Math.Pow(1 - t, 2) * topY + 2 * (1 - t) * t * controlY + Math.Pow(t, 2) * topY;
                    outline.Add(new XPoint(x, y));
                }

                // Zaokrąglij
                for (int i = 0; i < outline.Count; i++)
                {
                    outline[i] = new XPoint(Math.Round(outline[i].X, 4), Math.Round(outline[i].Y, 4));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GenerateCompleteOutline: {ex.Message}");
                return CreateFallbackRectangle();
            }

            return outline;
        }

        private void MarkGeometryDirty()
        {
            _geometryDirty = true;
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double topY = Y;

            // Punkt kontrolny dla łuku kwadratowego
            double controlX = leftX + Width / 2;
            double controlY = topY - ArcHeight;

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync(3);
            await ctx.BeginPathAsync();

            // Rysuj podobnie jak w preview
            await ctx.MoveToAsync(leftX, bottomY);                          // Lewy dolny
            await ctx.LineToAsync(rightX, bottomY);                         // Prawy dolny
            await ctx.LineToAsync(rightX, topY);                            // Prawy górny
            await ctx.QuadraticCurveToAsync(controlX, controlY, leftX, topY); // Łuk do lewego górnego
            await ctx.LineToAsync(leftX, bottomY);                          // Powrót do lewego dolnego

            await ctx.StrokeAsync();
        }

        public BoundingBox GetBoundingBox() => new BoundingBox(X, Y, Width, Height, NazwaObj);

        public List<XPoint> GetVertices() => GenerateCompleteOutline(IloscElementowLuki);

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GetVertices();
            var edges = new List<(XPoint, XPoint)>();
            for (int i = 0; i < v.Count - 1; i++)
                edges.Add((v[i], v[i + 1]));
            if (v.Count > 0)
                edges.Add((v[^1], v[0]));
            return edges;
        }

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 5) return;

            Points = newPoints.ToList();

            X = Points.Min(p => p.X);
            Y = Points.Min(p => p.Y);
            Width = Points.Max(p => p.X) - X;
            Height = Points.Max(p => p.Y) - Y;

            // Oblicz wysokość łuku z punktów
            var topPoints = Points.Where(p => Math.Abs(p.Y - Y) < 1).ToList();
            if (topPoints.Any())
            {
                double minTopY = topPoints.Min(p => p.Y);
                ArcHeight = Y - minTopY;
            }

            MarkGeometryDirty();
            CalculatePointsFromProperties();
        }

        public void Scale(double factor)
        {
            if (factor == 0) return;

            X *= factor;
            Y *= factor;
            Width *= factor;
            Height *= factor;
            ArcHeight *= factor;

            MarkGeometryDirty();
            CalculatePointsFromProperties();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();
            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }

        public void Transform(double scale, double offsetX, double offsetY) => Transform(scale, scale, offsetX, offsetY);

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            double scale = (scaleX + scaleY) / 2.0;

            X = X * scale + offsetX;
            Y = Y * scale + offsetY;
            Width = Math.Max(50, Width * scale);
            Height = Math.Max(50, Height * scale);
            ArcHeight = ArcHeight * scale;

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
                KsztaltModelu = KsztaltModelu,
            };
        }

        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("Pozycja X: ", () => X, v => { X = v; CalculatePointsFromProperties(); }, NazwaObj, true, false, false, true),
            new EditableProperty("Pozycja Y: ", () => Y, v => { Y = v; CalculatePointsFromProperties(); }, NazwaObj, true, false, false, true),
            new EditableProperty("Szerokość: ", () => Width, v => { Width = Math.Max(50, v); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Wysokość: ", () => Height, v => { Height = Math.Max(50, v); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Wysokość łuku: ", () => ArcHeight, v => { ArcHeight = v; CalculatePointsFromProperties(); }, NazwaObj),
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
            double topY = Y;

            var bottomLeft = new XPoint(leftX, bottomY);
            var bottomRight = new XPoint(rightX, bottomY);
            var topRight = new XPoint(rightX, topY);
            var topLeft = new XPoint(leftX, topY);

            segments.Add(new ContourSegment(bottomLeft, bottomRight));
            segments.Add(new ContourSegment(bottomRight, topRight));
            segments.Add(new ContourSegment(topRight, topLeft)); // Łuk będzie traktowany jako linia prosta dla uproszczenia
            segments.Add(new ContourSegment(topLeft, bottomLeft));

            return segments;
        }

        // Dodaj tę metodę do klasy XRoundedTopRectangleShapeFixed
        public (double centerX, double centerY, double startAngle, double endAngle) CalculateArcGeometry()
        {
            // Dla uproszczenia zwracamy domyślne wartości
            // Ponieważ kształt używa QuadraticCurveTo, nie potrzebujemy dokładnych kątów
            double centerX = X + Width / 2;
            double centerY = Y - ArcHeight;
            double startAngle = Math.PI;  // 180 stopni (lewa strona)
            double endAngle = 0;          // 0 stopni (prawa strona)

            return (centerX, centerY, startAngle, endAngle);
        }
    }
}